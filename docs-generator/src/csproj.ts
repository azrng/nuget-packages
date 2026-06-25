/**
 * .csproj 包元数据解析
 *
 * 从 .csproj 文件的 <Project><PropertyGroup> 中提取展示用元数据：
 * Title / PackageTags / Description / Version / TargetFrameworks
 *
 * 约束（已对仓库现状核实）：
 * - 仓库内的 .csproj 不使用 <PropertyGroup Condition="..."> 覆盖这些字段，
 *   故取最后一个出现的值即可，等价于 MSBuild 的「后定义覆盖前定义」语义。
 * - PackageTags / TargetFrameworks 均为单行分号分隔。
 * - 所有字段可选：缺失返回 undefined / 空数组，调用方按需兜底。
 */

import { XMLParser } from 'fast-xml-parser';

export interface ProjectMetadata {
  /** 包标题（缺失时由调用方用程序集名兜底） */
  title?: string;
  /** 标签数组（已去空、去重） */
  tags: string[];
  /** 一句话描述 */
  description?: string;
  /** 包版本，如 1.1.0 */
  version?: string;
  /** 目标框架数组，如 ['net6.0', 'net8.0'] */
  targetFrameworks: string[];
}

const xmlParser = new XMLParser({
  ignoreAttributes: false,
  attributeNamePrefix: '',
  textNodeName: '#text',
  parseAttributeValue: true,
  parseTagValue: true,
  trimValues: true,
});

/** 将分号分隔字符串拆为去空、去重的数组 */
function splitSemicolons(value: unknown): string[] {
  if (typeof value !== 'string') return [];
  const seen = new Set<string>();
  const result: string[] = [];
  for (const part of value.split(';')) {
    const trimmed = part.trim();
    if (trimmed && !seen.has(trimmed)) {
      seen.add(trimmed);
      result.push(trimmed);
    }
  }
  return result;
}

/** 从 PropertyGroup 节点对象中提取字符串值（兼容字符串与 {#text} 两种形态） */
function pickString(propGroup: Record<string, unknown>, key: string): string | undefined {
  const raw = propGroup[key];
  if (typeof raw === 'string') return raw.trim() || undefined;
  if (raw && typeof raw === 'object' && typeof (raw as any)['#text'] === 'string') {
    return (raw as any)['#text'].trim() || undefined;
  }
  return undefined;
}

/**
 * 解析 .csproj 内容，返回展示用元数据
 * 解析失败或无 PropertyGroup 时返回全空结果（不抛异常）
 */
export function parseCsprojMetadata(content: string): ProjectMetadata {
  const empty: ProjectMetadata = { tags: [], targetFrameworks: [] };
  if (!content) return empty;

  try {
    const doc = xmlParser.parse(content);
    const project = doc?.Project ?? doc;
    const propGroups = project?.PropertyGroup;
    if (!propGroups) return empty;

    // PropertyGroup 可能单个对象或数组；合并所有组，后者覆盖前者
    const groups: Record<string, unknown>[] = Array.isArray(propGroups)
      ? propGroups
      : [propGroups];
    const merged: Record<string, unknown> = {};
    for (const g of groups) {
      if (g && typeof g === 'object') Object.assign(merged, g);
    }

    // TargetFrameworks（复数，分号分隔）优先；否则取单数 TargetFramework
    const targetFrameworks = splitSemicolons(pickString(merged, 'TargetFrameworks'));
    const finalTargetFrameworks = targetFrameworks.length > 0
      ? targetFrameworks
      : splitSemicolons(pickString(merged, 'TargetFramework'));

    return {
      title: pickString(merged, 'Title'),
      tags: splitSemicolons(pickString(merged, 'PackageTags')),
      description: pickString(merged, 'Description'),
      version: pickString(merged, 'Version'),
      targetFrameworks: finalTargetFrameworks,
    };
  } catch {
    return empty;
  }
}
