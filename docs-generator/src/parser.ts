/**
 * 高性能 XML 解析器
 * 使用 fast-xml-parser 替代正则表达式，提升解析速度和准确性
 */

import { XMLParser } from 'fast-xml-parser';

// ==================== 类型定义 ====================

export interface Member {
  type: 'M' | 'P' | 'F' | 'E' | 'T';
  name: string;
  fullName: string;
  summary: string | null;
  params: ParamInfo[];
  typeParams: TypeParamInfo[];
  returns: string | null;
  remarks: string | null;
  namespace: string;
}

export interface ParamInfo {
  name: string;
  description: string;
}

export interface TypeParamInfo {
  name: string;
  description: string;
}

export interface TypeInfo extends Member {
  type: 'T';
}

export interface Assembly {
  fileName: string;
  name: string;
  namespaces: Map<string, Namespace>;
}

export interface Namespace {
  name: string;
  types: Map<string, TypeInfo>;
  members: Member[];
}

export interface ParsedData {
  assemblies: Assembly[];
  allTypes: TypeInfo[];
  allMembers: Member[];
  namespaces: Map<string, Namespace>;
}

// ==================== XML 解析器类 ====================

export class XmlParser {
  private assemblies = new Map<string, Assembly>();
  private xmlParser: XMLParser;

  constructor() {
    // 配置 fast-xml-parser
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
   */
  parseFile(content: string, fileName: string): void {
    try {
      const xmlDoc = this.xmlParser.parse(content);
      const doc = xmlDoc.doc || xmlDoc;

      const assemblyName = doc?.assembly?.name || doc?.name || fileName;

      if (!this.assemblies.has(fileName)) {
        this.assemblies.set(fileName, {
          fileName,
          name: assemblyName,
          namespaces: new Map()
        });
      }

      const assembly = this.assemblies.get(fileName)!;

      // 提取成员
      const members = doc?.members?.member;
      if (Array.isArray(members)) {
        members.forEach(member => {
          this.processMember(member, assembly);
        });
      } else if (members) {
        this.processMember(members, assembly);
      }
    } catch (error) {
      console.error(`Error parsing ${fileName}:`, error);
    }
  }

  /**
   * 处理单个成员节点
   */
  private processMember(memberNode: any, assembly: Assembly): void {
    if (!memberNode || !memberNode.name) return;

    const memberName = memberNode.name;
    const match = memberName.match(/^([A-Z]):(.+)$/);

    if (!match) return;

    const [, type, name] = match;

    // 只处理类型（T），跳过方法、属性、字段等
    // 这些成员会在类型详情中显示，不需要单独创建命名空间
    if (type !== 'T') {
      // 找到所属类型并添加为成员
      const typeName = this.extractTypeName(name);
      const namespace = this.extractNamespace(typeName);

      if (!assembly.namespaces.has(namespace)) {
        assembly.namespaces.set(namespace, {
          name: namespace,
          types: new Map(),
          members: []
        });
      }

      const ns = assembly.namespaces.get(namespace)!;

      // 如果类型存在，添加为成员
      if (ns.types.has(typeName)) {
        const member: Member = {
          type: type as Member['type'],
          name,
          fullName: memberName,
          summary: this.extractText(memberNode.summary),
          params: this.extractParamsFromName(name, memberNode.param),
          typeParams: this.extractTypeParams(memberNode.typeparam),
          returns: this.extractText(memberNode.returns),
          remarks: this.extractText(memberNode.remarks),
          namespace
        };
        ns.members.push(member);
      }
      return;
    }

    const namespace = this.extractNamespace(name);

    // 确保命名空间存在
    if (!assembly.namespaces.has(namespace)) {
      assembly.namespaces.set(namespace, {
        name: namespace,
        types: new Map(),
        members: []
      });
    }

    const ns = assembly.namespaces.get(namespace)!;

    // 创建类型对象
    const typeInfo: TypeInfo = {
      type: 'T',
      name,
      fullName: memberName,
      summary: this.extractText(memberNode.summary),
      params: [],
      typeParams: this.extractTypeParams(memberNode.typeparam),
      returns: null,
      remarks: this.extractText(memberNode.remarks),
      namespace
    };

    ns.types.set(name, typeInfo);
  }

  /**
   * 从方法名称中提取参数信息
   * 例如: "Namespace.Type.Method(System.Int32,System.String)"
   */
  private extractParamsFromName(memberName: string, paramNodes: any): ParamInfo[] {
    const params: ParamInfo[] = [];

    // 提取括号中的参数类型
    const parenOpen = memberName.indexOf('(');
    const parenClose = memberName.lastIndexOf(')');

    if (parenOpen > 0 && parenClose > parenOpen) {
      const paramTypesStr = memberName.substring(parenOpen + 1, parenClose);
      const paramTypes = paramTypesStr.split(',').map(t => t.trim()).filter(t => t);

      // 如果有param节点，使用param节点的name
      const paramNodesArray = Array.isArray(paramNodes) ? paramNodes : (paramNodes ? [paramNodes] : []);

      paramTypes.forEach((type, index) => {
        let paramName = '';
        let description = '';

        // 尝试从param节点获取参数名和描述
        if (paramNodesArray[index]) {
          const pNode = paramNodesArray[index];
          paramName = pNode.name || `arg${index}`;
          description = this.extractText(pNode) || '';
        } else {
          // 如果没有param节点，生成参数名
          paramName = `arg${index}`;
        }

        params.push({
          name: paramName,
          description: description || type
        });
      });
    }

    return params;
  }

  /**
   * 从成员名称中提取类型名称
   */
  private extractTypeName(memberName: string): string {
    // 移除方法签名部分
    const parenIndex = memberName.indexOf('(');
    let nameWithoutParams = memberName;
    if (parenIndex > 0) {
      nameWithoutParams = memberName.substring(0, parenIndex);
    }

    // 处理属性、字段、事件等
    // 例如: "Namespace.Type.Member" -> "Namespace.Type"
    const parts = nameWithoutParams.split('.');
    if (parts.length <= 1) return nameWithoutParams;

    // 移除最后一个部分（成员名）
    return parts.slice(0, -1).join('.');
  }

  /**
   * 提取命名空间
   */
  private extractNamespace(name: string): string {
    // 移除方法签名部分
    const parenIndex = name.indexOf('(');
    let cleanName = name;
    if (parenIndex > 0) {
      cleanName = name.substring(0, parenIndex);
    }

    const parts = cleanName.split('.');
    if (parts.length > 1) {
      // 返回除最后一个部分外的所有部分（命名空间）
      return parts.slice(0, -1).join('.');
    }
    return 'Global';
  }

  /**
   * 提取文本内容并清理 HTML 标签
   */
  private extractText(node: any): string | null {
    if (!node) return null;

    let text = typeof node === 'string' ? node : (node['#text'] || '');

    if (!text) return null;

    // 清理 HTML 实体和标签
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
   * 提取参数信息
   */
  private extractParams(paramNodes: any): ParamInfo[] {
    if (!paramNodes) return [];

    const params = Array.isArray(paramNodes) ? paramNodes : [paramNodes];

    return params
      .filter(p => p && p.name)
      .map(p => ({
        name: p.name,
        description: this.extractText(p) || ''
      }));
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
    const result: ParsedData = {
      assemblies: [],
      allTypes: [],
      allMembers: [],
      namespaces: new Map()
    };

    this.assemblies.forEach(assembly => {
      const assemblyData: Assembly = {
        fileName: assembly.fileName,
        name: assembly.name,
        namespaces: new Map()
      };

      assembly.namespaces.forEach((ns, nsName) => {
        const nsData: Namespace = {
          name: nsName,
          types: new Map(ns.types),
          members: [...ns.members]
        };

        assemblyData.namespaces.set(nsName, nsData);
        result.allTypes.push(...Array.from(ns.types.values()));
        result.allMembers.push(...ns.members);

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
    this.assemblies.clear();
  }
}
