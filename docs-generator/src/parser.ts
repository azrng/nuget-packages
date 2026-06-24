/**
 * 高性能 XML 解析器
 * 使用 fast-xml-parser 替代正则表达式，提升解析速度和准确性
 *
 * 数据模型说明：
 * 成员（方法/属性/字段/事件）直接挂在其所属 Type 的 members[] 上，
 * 而不是用扁平数组 + 字符串前缀反查。这样：
 * 1. 解决泛型类型成员归属问题（Foo`1 与 Foo{T}.Method 的前缀不匹配）
 * 2. 解决重载方法误匹配问题
 * 3. 解决 XML 中成员排在类型定义前导致静默丢失的问题（两遍扫描）
 */

import { XMLParser } from 'fast-xml-parser';
import { getCategory, Category } from './utils.js';

// ==================== 类型定义 ====================

export type MemberType = 'M' | 'P' | 'F' | 'E' | 'T';

export interface Member {
  type: Exclude<MemberType, 'T'>;
  name: string;        // XML 中的完整成员名（含命名空间，方法含参数签名）
  fullName: string;    // M:.... 形式
  shortName: string;   // 去掉所属类型前缀后的成员名
  summary: string | null;
  params: ParamInfo[];
  typeParams: TypeParamInfo[];
  returns: string | null;
  remarks: string | null;
  namespace: string;
}

export interface ParamInfo {
  name: string;
  type: string;        // 参数类型（从成员名签名的 (...) 中解析）
  description: string;
}

export interface TypeParamInfo {
  name: string;
  description: string;
}

export interface TypeInfo {
  type: 'T';
  name: string;        // 完整类型名（含命名空间，如 Azrng.Core.Helpers.StringHelper）
  fullName: string;    // T:.... 形式
  shortName: string;   // 不含命名空间的短名
  category: Category;
  namespace: string;
  summary: string | null;
  remarks: string | null;
  typeParams: TypeParamInfo[];
  members: Member[];   // 成员直接挂在类型上
}

export interface Assembly {
  fileName: string;
  name: string;
  namespaces: Map<string, Namespace>;
}

export interface Namespace {
  name: string;
  types: Map<string, TypeInfo>;
}

export interface ParsedData {
  assemblies: Assembly[];
  allTypes: TypeInfo[];
  allMembers: Member[];
  namespaces: Map<string, Namespace>;
}

// ==================== 原始成员节点（解析阶段临时结构） ====================

interface RawTypeNode {
  fileName: string;
  assembly: Assembly;
  memberName: string;   // T:....
  name: string;         // 类型名
  namespace: string;
  node: any;
}

interface RawMemberNode {
  fileName: string;
  assembly: Assembly;   // 所属类库（用于成员归属时定位类型）
  type: Exclude<MemberType, 'T'>;
  typeName: string;     // 归属的类型名（去掉成员名和参数签名）
  namespace: string;
  name: string;         // XML 完整成员名
  node: any;
}

// ==================== XML 解析器类 ====================

export class XmlParser {
  private xmlParser: XMLParser;
  private rawTypes: RawTypeNode[] = [];
  private rawMembers: RawMemberNode[] = [];
  private assemblyMap = new Map<string, Assembly>();

  constructor() {
    this.xmlParser = new XMLParser({
      ignoreAttributes: false,
      attributeNamePrefix: '',
      textNodeName: '#text',
      parseAttributeValue: true,
      parseTagValue: true,
      trimValues: true,
      stopNodes: ['*.summary', '*.remarks', '*.returns', '*.param', '*.exception', '*.example']
    });
  }

  /**
   * 解析单个 XML 文件
   * 第一遍：只收集原始节点（类型节点 + 成员节点），不建立归属关系
   */
  parseFile(content: string, fileName: string): void {
    try {
      const xmlDoc = this.xmlParser.parse(content);
      const doc = xmlDoc.doc || xmlDoc;

      const assemblyName = doc?.assembly?.name || doc?.name || fileName;

      if (!this.assemblyMap.has(fileName)) {
        this.assemblyMap.set(fileName, {
          fileName,
          name: assemblyName,
          namespaces: new Map()
        });
      }
      const assembly = this.assemblyMap.get(fileName)!;

      const membersNode = doc?.members?.member;
      const memberList: any[] = Array.isArray(membersNode)
        ? membersNode
        : (membersNode ? [membersNode] : []);

      for (const memberNode of memberList) {
        if (!memberNode || !memberNode.name) continue;

        const memberName: string = memberNode.name;
        const match = memberName.match(/^([A-Z]):(.+)$/);
        if (!match) continue;

        const [, prefix, name] = match;

        if (prefix === 'T') {
          // 类型节点
          const namespace = this.extractNamespace(name);
          this.ensureNamespace(assembly, namespace);
          this.rawTypes.push({
            fileName, assembly, memberName, name, namespace, node: memberNode
          });
        } else {
          // 成员节点：计算归属类型名
          const typeName = this.extractTypeName(name);
          const namespace = this.extractNamespace(typeName);
          this.ensureNamespace(assembly, namespace);
          this.rawMembers.push({
            fileName,
            assembly,
            type: prefix as Exclude<MemberType, 'T'>,
            typeName,
            namespace,
            name,
            node: memberNode
          });
        }
      }
    } catch (error) {
      console.error(`Error parsing ${fileName}:`, error);
    }
  }

  private ensureNamespace(assembly: Assembly, namespace: string): void {
    if (!assembly.namespaces.has(namespace)) {
      assembly.namespaces.set(namespace, { name: namespace, types: new Map() });
    }
  }

  /**
   * 第二遍：建立类型对象，并把成员归属到对应类型
   * 调用 getAllData() 时触发
   */
  private buildTypes(): void {
    // 1. 先建立所有类型对象
    for (const raw of this.rawTypes) {
      const { assembly, memberName, name, namespace, node } = raw;
      const ns = assembly.namespaces.get(namespace)!;

      const typeInfo: TypeInfo = {
        type: 'T',
        name,
        fullName: memberName,
        shortName: name.split('.').pop() || name,
        category: getCategory(name),
        namespace,
        summary: this.extractText(node.summary),
        remarks: this.extractText(node.remarks),
        typeParams: this.extractTypeParams(node.typeparam),
        members: []
      };
      ns.types.set(name, typeInfo);
    }

    // 2. 再把成员归属到类型（此时所有类型已就绪，无论 XML 中成员是否排在类型前）
    for (const raw of this.rawMembers) {
      const { assembly, typeName, name, node, type } = raw;
      const typeInfo = assembly.namespaces.get(raw.namespace)?.types.get(typeName);

      // 仅当所属类型存在时才记录成员（避免孤立的成员节点污染数据）
      if (!typeInfo) continue;

      const member: Member = {
        type,
        name,
        fullName: `${type}:${name}`,
        shortName: this.extractMemberShortName(name, typeName),
        summary: this.extractText(node.summary),
        params: this.extractParams(name, node.param),
        typeParams: type === 'M' ? this.extractTypeParams(node.typeparam) : [],
        returns: this.extractText(node.returns),
        remarks: this.extractText(node.remarks),
        namespace: raw.namespace
      };
      typeInfo.members.push(member);
    }
  }

  /**
   * 从成员的完整名中提取短名（去掉所属类型前缀）
   * 例：Azrng.Core.Helpers.StringHelper.IsNullOrEmpty(System.String) -> IsNullOrEmpty
   */
  private extractMemberShortName(memberName: string, typeName: string): string {
    // 去掉参数签名
    const parenIndex = memberName.indexOf('(');
    const nameOnly = parenIndex > 0 ? memberName.substring(0, parenIndex) : memberName;
    // 去掉类型前缀 "TypeName."
    const prefix = typeName + '.';
    if (nameOnly.startsWith(prefix)) {
      return nameOnly.substring(prefix.length);
    }
    return nameOnly.split('.').pop() || nameOnly;
  }

  /**
   * 从成员名称中提取所属类型名
   * 例：Azrng.Core.Helpers.StringHelper.IsNullOrEmpty(...) -> Azrng.Core.Helpers.StringHelper
   *     Azrng.Core.Helpers.StringHelper.Length -> Azrng.Core.Helpers.StringHelper
   */
  private extractTypeName(memberName: string): string {
    // 去掉方法签名部分（括号及之后）
    const parenIndex = memberName.indexOf('(');
    const nameWithoutParams = parenIndex > 0 ? memberName.substring(0, parenIndex) : memberName;

    const parts = nameWithoutParams.split('.');
    if (parts.length <= 1) return nameWithoutParams;
    // 去掉最后一段（成员名），剩余即类型名
    return parts.slice(0, -1).join('.');
  }

  /**
   * 提取命名空间
   * 例：Azrng.Core.Helpers.StringHelper -> Azrng.Core.Helpers
   */
  private extractNamespace(name: string): string {
    const parenIndex = name.indexOf('(');
    const cleanName = parenIndex > 0 ? name.substring(0, parenIndex) : name;
    const parts = cleanName.split('.');
    if (parts.length > 1) {
      return parts.slice(0, -1).join('.');
    }
    return 'Global';
  }

  /**
   * 解析方法参数：结合成员名签名中的参数类型 + <param> 节点的参数名/描述
   *
   * 关键修复：按 `{}` 深度计数 split 参数类型，避免误切泛型内部逗号。
   * 例：Method(List{System.String}, System.Int32) -> 两个参数，而非把 List{System.String} 拆开
   */
  private extractParams(memberName: string, paramNodes: any): ParamInfo[] {
    const params: ParamInfo[] = [];

    const parenOpen = memberName.indexOf('(');
    const parenClose = memberName.lastIndexOf(')');
    if (parenOpen <= 0 || parenClose <= parenOpen) return params;

    const paramTypesStr = memberName.substring(parenOpen + 1, parenClose).trim();
    if (!paramTypesStr) return params;

    // 按泛型 {} 深度计数 split，泛型内部的逗号不拆分
    const paramTypes = this.splitTopLevel(paramTypesStr, ['{', '}']);

    const paramNodesArray = Array.isArray(paramNodes)
      ? paramNodes
      : (paramNodes ? [paramNodes] : []);

    paramTypes.forEach((type, index) => {
      let paramName = '';
      let description = '';

      if (paramNodesArray[index]) {
        const pNode = paramNodesArray[index];
        paramName = pNode.name || `arg${index}`;
        description = this.extractText(pNode) || '';
      } else {
        paramName = `arg${index}`;
      }

      params.push({
        name: paramName,
        type: type.trim(),
        description
      });
    });

    return params;
  }

  /**
   * 按层级分隔符 split 字符串
   * open/close 成对，遇到 open 深度 +1，close 深度 -1，仅在深度为 0 时按 sep 分割
   * 用于正确处理泛型 List{A,B}、嵌套括号等场景
   */
  private splitTopLevel(str: string, pair: [string, string], sep: string = ','): string[] {
    const result: string[] = [];
    let depth = 0;
    let current = '';

    for (const ch of str) {
      if (ch === pair[0]) {
        depth++;
        current += ch;
      } else if (ch === pair[1]) {
        depth = Math.max(0, depth - 1);
        current += ch;
      } else if (ch === sep && depth === 0) {
        result.push(current);
        current = '';
      } else {
        current += ch;
      }
    }
    if (current.trim()) {
      result.push(current);
    }
    return result;
  }

  /**
   * 提取文本内容并清理 HTML 标签
   */
  private extractText(node: any): string | null {
    if (!node) return null;

    let text = typeof node === 'string' ? node : (node['#text'] || '');
    if (!text) return null;

    return text
      .trim()
      .replace(/<[^>]+>/g, '')
      .replace(/&lt;/g, '<')
      .replace(/&gt;/g, '>')
      .replace(/&amp;/g, '&')
      .replace(/&quot;/g, '"')
      .replace(/\s+/g, ' ') || null;
  }

  /**
   * 提取类型参数信息
   */
  private extractTypeParams(typeParamNodes: any): TypeParamInfo[] {
    if (!typeParamNodes) return [];

    const params = Array.isArray(typeParamNodes) ? typeParamNodes : [typeParamNodes];

    return params
      .filter(p => p && p.name)
      .map(p => ({
        name: p.name,
        description: this.extractText(p) || ''
      }));
  }

  /**
   * 获取所有解析后的数据
   */
  getAllData(): ParsedData {
    this.buildTypes();

    const result: ParsedData = {
      assemblies: [],
      allTypes: [],
      allMembers: [],
      namespaces: new Map()
    };

    this.assemblyMap.forEach(assembly => {
      const assemblyData: Assembly = {
        fileName: assembly.fileName,
        name: assembly.name,
        namespaces: new Map()
      };

      assembly.namespaces.forEach((ns, nsName) => {
        if (ns.types.size === 0) return;

        const nsData: Namespace = {
          name: nsName,
          types: new Map()
        };

        ns.types.forEach((typeInfo, typeName) => {
          nsData.types.set(typeName, typeInfo);
          result.allTypes.push(typeInfo);
          result.allMembers.push(...typeInfo.members);
        });

        assemblyData.namespaces.set(nsName, nsData);

        if (!result.namespaces.has(nsName)) {
          result.namespaces.set(nsName, nsData);
        }
      });

      result.assemblies.push(assemblyData);
    });

    return result;
  }

  /**
   * 清空解析器状态
   */
  clear(): void {
    this.rawTypes = [];
    this.rawMembers = [];
    this.assemblyMap.clear();
  }
}
