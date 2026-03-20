/**
 * API 文档生成器 - 主入口
 * 为 NuGetPackages 项目自动生成 API 文档
 */

import * as fs from 'fs';
import * as path from 'path';
import { XMLParser } from 'fast-xml-parser';
import { generateHTML } from './generator.js';

const CONFIG = {
  xmlSourceDir: path.join(process.cwd(), 'src'),
  outputDir: path.join(process.cwd(), 'docs'),
  outputFile: 'index.html'
};

// ... (完整的代码，与上面相同)
