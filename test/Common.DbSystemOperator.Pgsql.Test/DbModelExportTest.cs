using Azrng.Core.Extension;
using Azrng.DbOperator;
using Common.DbSystemOperator.Pgsql.Test.Model;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace Common.DbSystemOperator.Pgsql.Test;

/// <summary>
/// 数据库模型导出测试
/// </summary>
public class DbModelExportTest
{
    private readonly IBasicDbBridge _basicDbBridge;

    public DbModelExportTest(IBasicDbBridge basicDbBridge)
    {
        _basicDbBridge = basicDbBridge;
    }

    /// <summary>
    /// 模型导出测试
    /// </summary>
    [Fact]
    public async Task ExportModel()
    {
        var fileName = DateTime.Now.ToFormatString("HHmmss") + ".xlsx";
        var targetSchema = "manager";

        var exportModel = await BuildExportModelAsync(targetSchema);
        var buffer = DataTableToExcelProExport(exportModel);
        await File.WriteAllBytesAsync(fileName, buffer);
    }

    /// <summary>
    /// 构建导出模型
    /// </summary>
    /// <param name="targetSchema">目标架构名称，为空则导出所有架构</param>
    /// <returns>导出模型信息</returns>
    private async Task<ModelExportProInfo> BuildExportModelAsync(string targetSchema)
    {
        var exportModel = new ModelExportProInfo();
        var schemaList = await _basicDbBridge.GetSchemaListAsync();
        var filteredSchemas = string.IsNullOrWhiteSpace(targetSchema)
            ? schemaList
            : schemaList.Where(s => s.SchemaName == targetSchema).ToList();

        foreach (var schema in filteredSchemas)
        {
            var schemaSheetExport = new SchemaSheetExport { SchemaRemark = schema.SchemaComment, SchemaName = schema.SchemaName };

            var schemaCategory = new DataTableCategorySchemaExport { SchemaName = schema.SchemaName, SchemaCnName = schema.SchemaComment };

            await ProcessTablesAsync(schema.SchemaName, schemaSheetExport, schemaCategory);
            await ProcessViewsAsync(schema.SchemaName, schemaSheetExport, schemaCategory);
            await ProcessProceduresAsync(schema.SchemaName, schemaSheetExport, schemaCategory);

            exportModel.DataTableCategorySheet.Add(schemaCategory);
            exportModel.SchemaSheets.Add(schemaSheetExport);
        }

        return exportModel;
    }

    /// <summary>
    /// 处理表信息
    /// </summary>
    private async Task ProcessTablesAsync(string schemaName, SchemaSheetExport schemaSheetExport,
                                          DataTableCategorySchemaExport schemaCategory)
    {
        // 获取表信息
        var tableList = await _basicDbBridge.GetTableInfoListAsync(schemaName);
        foreach (var table in tableList)
        {
            var schemaSheetTable = new SchemaSheetTableExport { TableName = table.TableName, TableComment = table.TableComment };

            schemaCategory.SchemaStructModelExports.Add(new SchemaStructModelExport
                                                        {
                                                            StructTypeName = "表",
                                                            StructModelName = table.TableName,
                                                            StructModelCnName = table.TableComment
                                                        });

            // 查询列
            var columnList = await _basicDbBridge.GetColumnListAsync(schemaName, table.TableName);
            foreach (var column in columnList)
            {
                schemaSheetTable.SchemaSheetTableColumns.Add(new SchemaSheetTableColumnExport
                                                             {
                                                                 ColumnName = column.ColumnName,
                                                                 ColumnCnName = "",
                                                                 ColumnType = column.ColumnType,
                                                                 DefaultValue = HandlerDefaultValue(column.ColumnDefault),
                                                                 IsPrimary = column.IsPrimaryKey ? "是" : "否",
                                                                 IsNotNull = column.IsNull ? "否" : "是",
                                                                 IsForeignKey = column.IsForeignKey ? "是" : "否",
                                                                 Comment = column.ColumnComment ?? string.Empty
                                                             });
            }

            schemaSheetExport.SchemaSheetTables.Add(schemaSheetTable);
        }
    }

    /// <summary>
    /// 处理视图信息
    /// </summary>
    private async Task ProcessViewsAsync(string schemaName, SchemaSheetExport schemaSheetExport,
                                         DataTableCategorySchemaExport schemaCategory)
    {
        // 获取视图信息
        var viewList = await _basicDbBridge.GetSchemaViewListAsync(schemaName);
        foreach (var view in viewList)
        {
            schemaSheetExport.SchemaSheetViews.Add(new SchemaSheetViewExport
                                                   {
                                                       ViewName = view.ViewName,
                                                       ViewCnName = view.ViewDescription,
                                                       ViewComment = view.ViewDescription,
                                                       CreateSqlStr = view.ViewDefinition
                                                   });

            schemaCategory.SchemaStructModelExports.Add(new SchemaStructModelExport
                                                        {
                                                            StructTypeName = "视图",
                                                            StructModelName = view.ViewName,
                                                            StructModelCnName = view.ViewDescription
                                                        });
        }
    }

    /// <summary>
    /// 处理存储过程信息
    /// </summary>
    private async Task ProcessProceduresAsync(string schemaName, SchemaSheetExport schemaSheetExport,
                                              DataTableCategorySchemaExport schemaCategory)
    {
        // 获取存储过程信息
        var procList = await _basicDbBridge.GetSchemaProcListAsync(schemaName);
        foreach (var proc in procList)
        {
            schemaSheetExport.SchemaSheetProcList.Add(new SchemaSheetProcExport
                                                      {
                                                          ProcName = proc.ProcName,
                                                          ProcCnName = proc.ProcDescription,
                                                          ProcComment = proc.ProcDescription,
                                                          CreateSqlStr = proc.ProcDefinition
                                                      });

            schemaCategory.SchemaStructModelExports.Add(new SchemaStructModelExport
                                                        {
                                                            StructTypeName = "存储过程",
                                                            StructModelName = proc.ProcName,
                                                            StructModelCnName = proc.ProcDescription
                                                        });
        }
    }

    /// <summary>
    /// 导出新模板
    /// </summary>
    /// <param name="modelExportProInfo"></param>
    /// <returns></returns>
    private byte[] DataTableToExcelProExport(ModelExportProInfo modelExportProInfo)
    {
        using var workbook = new XSSFWorkbook();
        var tablesSheetName = "数据对象目录";
        var locatorDict = new Dictionary<string, string>();

        if (modelExportProInfo.SchemaSheets.Count > 0)
        {
            modelExportProInfo.SchemaSheets.OrderBy(p => p.SchemaName)
                              .ToList()
                              .ForEach(p =>
                              {
                                  BuildSchemaSheet(workbook,
                                      tablesSheetName,
                                      p.SchemaName,
                                      p.SchemaRemark,
                                      p.SchemaSheetTables.OrderBy(o => o.TableName).ToList(),
                                      p.SchemaSheetViews.OrderBy(o => o.ViewName).ToList(),
                                      p.SchemaSheetProcList.OrderBy(o => o.ProcName).ToList(),
                                      locatorDict);
                              });
        }

        BuildTableCategorySheet(workbook, tablesSheetName, modelExportProInfo.DataTableCategorySheet, locatorDict);

        workbook.SetSheetOrder(tablesSheetName, 0);
        workbook.SetActiveSheet(0);
        workbook.SetSelectedTab(0);

        using var ms = new MemoryStream();
        workbook.Write(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// 渲染数据对象目录sheet
    /// </summary>
    private void BuildTableCategorySheet(IWorkbook workbook, string tablesSheetName,
                                         List<DataTableCategorySchemaExport> dataTableCategoryExport,
                                         Dictionary<string, string> locatorDict)
    {
        var sheet = workbook.CreateSheet(tablesSheetName) as XSSFSheet;
        var nextRowIndexInSheet = 0;

        if (!dataTableCategoryExport.Any())
        {
            sheet.CreateRow(0).CreateCell(0).SetCellValue("(无数据)");
            return;
        }

        var bodystyle = GetBodyStyle(workbook, IndexedColors.White.Index);
        var headerCellStyle = GetHeaderStyle(workbook);

        var headers = new List<string>
                      {
                          "业务域名",
                          "业务域中文名",
                          "类型",
                          "对象名称",
                          "对象中文名"
                      };

        var totalColumnSize = headers.Count;
        var totalRowCount = dataTableCategoryExport.SelectMany(x => x.SchemaStructModelExports).Count();

        for (var rh = 0; rh <= totalRowCount; rh++)
        {
            var schemaHeaderRow = sheet.CreateRow(nextRowIndexInSheet + rh);
            for (var c = 0; c < totalColumnSize; ++c)
            {
                schemaHeaderRow.CreateCell(c).CellStyle = headerCellStyle;
            }
        }

        var sortSchemas = dataTableCategoryExport.OrderBy(p => p.SchemaName).ToList();
        foreach (var (schema, index) in sortSchemas.WithIndex())
        {
            if (index == 0)
            {
                var columnRow = sheet.GetRow(index);
                foreach (var (head, hindex) in headers.WithIndex())
                {
                    columnRow.GetCell(hindex).SetCellValue(head);
                }

                nextRowIndexInSheet++;
            }

            var dataRow = sheet.GetRow(nextRowIndexInSheet);
            var schemaTotalStructCount = schema.SchemaStructModelExports.Count;

            var schemaTableStartRowIndex =
                schema.SchemaStructModelExports.FindIndex(p => p.StructTypeName == "表") + nextRowIndexInSheet;
            var schemaTableEndRowIndex =
                schema.SchemaStructModelExports.FindLastIndex(p => p.StructTypeName == "表") + nextRowIndexInSheet;
            var schemaViewStartRowIndex =
                schema.SchemaStructModelExports.FindIndex(p => p.StructTypeName == "视图") + nextRowIndexInSheet;
            var schemaViewEndRowIndex =
                schema.SchemaStructModelExports.FindLastIndex(p => p.StructTypeName == "视图") + nextRowIndexInSheet;
            var schemaProcStartRowIndex =
                schema.SchemaStructModelExports.FindIndex(p => p.StructTypeName == "存储过程") + nextRowIndexInSheet;
            var schemaProcEndRowIndex =
                schema.SchemaStructModelExports.FindLastIndex(p => p.StructTypeName == "存储过程") + nextRowIndexInSheet;

            var schemaCell = dataRow.GetCell(0);
            schemaCell.CellStyle = bodystyle;
            schemaCell.SetCellValue(schema.SchemaName);

            var schemaCnCell = dataRow.GetCell(1);
            schemaCnCell.CellStyle = bodystyle;
            schemaCnCell.SetCellValue(string.IsNullOrWhiteSpace(schema.SchemaCnName) ? schema.SchemaName : schema.SchemaCnName);

            var mergeSchemaEndRowIndex = nextRowIndexInSheet + schemaTotalStructCount - 1;
            if (mergeSchemaEndRowIndex > nextRowIndexInSheet)
            {
                sheet.AddMergedRegion(new CellRangeAddress(nextRowIndexInSheet, mergeSchemaEndRowIndex, 0, 0));
                sheet.AddMergedRegion(new CellRangeAddress(nextRowIndexInSheet, mergeSchemaEndRowIndex, 1, 1));

                if (schemaTableStartRowIndex >= 1 && schemaTableEndRowIndex >= 1 && schemaTableStartRowIndex != schemaTableEndRowIndex)
                    sheet.AddMergedRegion(new CellRangeAddress(schemaTableStartRowIndex, schemaTableEndRowIndex, 2, 2));
                if (schemaViewStartRowIndex >= 1 && schemaViewEndRowIndex >= 1 && schemaViewStartRowIndex != schemaViewEndRowIndex)
                    sheet.AddMergedRegion(new CellRangeAddress(schemaViewStartRowIndex, schemaViewEndRowIndex, 2, 2));
                if (schemaProcStartRowIndex >= 1 && schemaProcEndRowIndex >= 1 && schemaProcStartRowIndex != schemaProcEndRowIndex)
                    sheet.AddMergedRegion(new CellRangeAddress(schemaProcStartRowIndex, schemaProcEndRowIndex, 2, 2));
            }

            var dataTable = schema.SchemaStructModelExports.ToDataTable();
            for (var x = 0; x < dataTable.Rows.Count; ++x)
            {
                var row = (x == 0) ? dataRow : sheet.GetRow(nextRowIndexInSheet);

                for (var j = 2; j < headers.Count; ++j)
                {
                    var cell = row.GetCell(j);
                    var rawStr = dataTable.Rows[x][j - 2].ToString();

                    if (int.TryParse(rawStr, out var value))
                    {
                        cell.SetCellValue(value);
                    }
                    else
                    {
                        cell.SetCellValue(rawStr);
                    }

                    var tableName = dataTable.Columns[j - 2].ColumnName;
                    if (tableName == "对象名称")
                    {
                        var locatorKey = $"{schema.SchemaName}-{rawStr}";
                        if (locatorDict.TryGetValue(locatorKey, out var address))
                        {
                            CreateHyperLink(workbook, cell, $"{address}");
                        }
                    }

                    cell.CellStyle = bodystyle;
                }

                nextRowIndexInSheet++;
            }
        }

        sheet.SetColumnWidth(0, 15 * 256);
        sheet.SetColumnWidth(1, 18 * 256);
        sheet.SetColumnWidth(2, 35 * 256);
        sheet.SetColumnWidth(3, 35 * 256);
        sheet.SetColumnWidth(4, 13 * 256);
    }

    /// <summary>
    /// 渲染每个域sheet
    /// </summary>
    private void BuildSchemaSheet(IWorkbook workbook,
                                  string tablesSheetName,
                                  string schemaName,
                                  string schemaCnName,
                                  List<SchemaSheetTableExport> schemaSheetTables,
                                  List<SchemaSheetViewExport> schemaSheetViews,
                                  List<SchemaSheetProcExport> schemaSheetProcs,
                                  Dictionary<string, string> locatorDict)
    {
        var sheet = workbook.CreateSheet(schemaName) as XSSFSheet;
        var bodystyle = GetBodyStyle(workbook, IndexedColors.White.Index);
        var headerCellStyle = GetHeaderStyle(workbook);

        var nextRowIndexInSheet = 0;

        CreateHyperLink(workbook, sheet.CreateRow(nextRowIndexInSheet).CreateCell(1), $"'{tablesSheetName}'!A1", "返回");

        var colsHeaders = new List<string>
                          {
                              "序号",
                              "字段名",
                              "中文名",
                              "字段类型",
                              "字段说明",
                              "默认值",
                              "主键",
                              "外键",
                              "非空"
                          };
        var tableSqlHeader = "建表sql";

        var totalColumnSize = colsHeaders.Count;

        foreach (var (table, index) in schemaSheetTables.WithIndex())
        {
            nextRowIndexInSheet += 2;

            var totalRowsCount = table.SchemaSheetTableColumns.Count + 3;

            for (var rh = 0; rh < totalRowsCount; rh++)
            {
                var schemaHeaderRow = sheet.CreateRow(nextRowIndexInSheet + rh);
                for (var c = 0; c < totalColumnSize; ++c)
                {
                    schemaHeaderRow.CreateCell(c).CellStyle = headerCellStyle;
                }
            }

            locatorDict.Add($"{schemaName}-{table.TableName}", $"'{schemaName}'!A{nextRowIndexInSheet + 1}");

            nextRowIndexInSheet = FillFirstRowSection(sheet, nextRowIndexInSheet, table.TableName, table.TableComment);
            nextRowIndexInSheet = FillFixColumnValueSection(schemaName, schemaCnName, sheet, nextRowIndexInSheet, table.TableComment);
            nextRowIndexInSheet = FillTableDataInfoSection(sheet, bodystyle, nextRowIndexInSheet, colsHeaders, table);
            nextRowIndexInSheet = FillIndexSection(sheet, bodystyle, headerCellStyle, nextRowIndexInSheet, totalColumnSize,
                table);
            nextRowIndexInSheet = SqlScriptSection(sheet, bodystyle, headerCellStyle, nextRowIndexInSheet, tableSqlHeader,
                totalColumnSize, table.CreateSqlStr);

            sheet.SetColumnWidth(0, 10 * 256);
            sheet.SetColumnWidth(1, 19 * 256);
            sheet.SetColumnWidth(2, 28 * 256);
            sheet.SetColumnWidth(3, 20 * 256);
            sheet.SetColumnWidth(4, 20 * 256);
            sheet.SetColumnWidth(5, 35 * 256);
            sheet.SetColumnWidth(6, 18 * 256);
            sheet.SetColumnWidth(7, 10 * 256);
            sheet.SetColumnWidth(8, 10 * 256);
            sheet.SetColumnWidth(9, 10 * 256);

            nextRowIndexInSheet++;
        }

        var viewSqlHeader = "视图sql";
        var procSqlHeader = "存储过程sql";

        foreach (var (view, index) in schemaSheetViews.WithIndex())
        {
            nextRowIndexInSheet += 2;

            CreateRowAheadOfTime(sheet, headerCellStyle, nextRowIndexInSheet, totalColumnSize);

            nextRowIndexInSheet = FillViewAndProcInfo(schemaName,
                schemaCnName,
                locatorDict,
                sheet,
                bodystyle,
                headerCellStyle,
                nextRowIndexInSheet,
                totalColumnSize,
                viewSqlHeader,
                view.ViewName,
                view.ViewCnName,
                view.CreateSqlStr,
                view.ViewComment);

            nextRowIndexInSheet++;
        }

        foreach (var (proc, index) in schemaSheetProcs.WithIndex())
        {
            nextRowIndexInSheet += 2;

            CreateRowAheadOfTime(sheet, headerCellStyle, nextRowIndexInSheet, totalColumnSize);

            nextRowIndexInSheet = FillViewAndProcInfo(schemaName,
                schemaCnName,
                locatorDict,
                sheet,
                bodystyle,
                headerCellStyle,
                nextRowIndexInSheet,
                totalColumnSize,
                procSqlHeader,
                proc.ProcName,
                proc.ProcCnName,
                proc.CreateSqlStr,
                proc.ProcComment);

            nextRowIndexInSheet++;
        }
    }

    /// <summary>
    /// 填充表数据
    /// </summary>
    private int FillTableDataInfoSection(XSSFSheet sheet, ICellStyle bodyStyle, int nextRowIndexInSheet, List<string> colsHeaders,
                                         SchemaSheetTableExport table)
    {
        var headRowCol3 = sheet.GetRow(nextRowIndexInSheet);
        foreach (var (head, chindex) in colsHeaders.WithIndex())
        {
            headRowCol3.GetCell(chindex).SetCellValue(head);
        }

        nextRowIndexInSheet++;

        var sortCols = table.SchemaSheetTableColumns.OrderByDescending(p => p.IsPrimary).ThenByDescending(p => p.IsForeignKey).ToList();
        var dataTableCols = sortCols.ToDataTable();
        for (var x = 0; x < dataTableCols.Rows.Count; ++x)
        {
            var row = sheet.GetRow(nextRowIndexInSheet);
            for (var j = 0; j < colsHeaders.Count; ++j)
            {
                var cell = row.GetCell(j);
                var rawStr = j == 0 ? (x + 1).ToString() : dataTableCols.Rows[x][j - 1].ToString();
                cell.SetCellValue(rawStr);
                cell.CellStyle = bodyStyle;
            }

            nextRowIndexInSheet++;
        }

        return nextRowIndexInSheet;
    }

    /// <summary>
    /// 填充视图和存储过程信息
    /// </summary>
    private int FillViewAndProcInfo(string schemaName,
                                    string schemaCnName,
                                    Dictionary<string, string> locatorDict,
                                    XSSFSheet sheet,
                                    ICellStyle bodyStyle,
                                    ICellStyle headerCellStyle,
                                    int nextRowIndexInSheet,
                                    int totalColumnSize,
                                    string sqlHeader,
                                    string structName,
                                    string structCnName,
                                    string createSqlStr,
                                    string remark)
    {
        locatorDict.Add($"{schemaName}-{structName}", $"'{schemaName}'!A{nextRowIndexInSheet + 1}");

        nextRowIndexInSheet = FillFirstRowSection(sheet, nextRowIndexInSheet, structName, structCnName);
        nextRowIndexInSheet = FillFixColumnValueSection(schemaName, schemaCnName, sheet, nextRowIndexInSheet, remark);
        nextRowIndexInSheet = SqlScriptSection(sheet, bodyStyle, headerCellStyle, nextRowIndexInSheet, sqlHeader,
            totalColumnSize, createSqlStr);

        return nextRowIndexInSheet;
    }

    private int SqlScriptSection(XSSFSheet sheet, ICellStyle bodyStyle, ICellStyle headerCellStyle, int nextRowIndexInSheet,
                                 string sqlHeader, int totalColumnSize, string sqlStr)
    {
        if (string.IsNullOrWhiteSpace(sqlStr))
        {
            return nextRowIndexInSheet;
        }

        var sqlStrRowsExtra = CalCellExtraLine(sqlStr);
        var sqlStrRowsTotal = sqlStrRowsExtra + 1;
        var sqlStrRowsStartIndex = nextRowIndexInSheet;

        for (var rh = 0; rh < sqlStrRowsTotal; rh++)
        {
            var tmpHeaderRow = sheet.CreateRow(nextRowIndexInSheet);
            tmpHeaderRow.Height = 2000;

            for (var c = 0; c < totalColumnSize; ++c)
            {
                tmpHeaderRow.CreateCell(c).CellStyle = headerCellStyle;
            }

            if (rh == 0)
            {
                tmpHeaderRow.GetCell(0).SetCellValue(sqlHeader);
            }

            nextRowIndexInSheet++;
        }

        var max = 32767;
        var num = sqlStr.Length / max;
        for (var i = 0; i < num; i++)
        {
            var cRowIndex = sqlStrRowsStartIndex + i;
            var cell = sheet.GetRow(cRowIndex).GetCell(1);
            cell.CellStyle = bodyStyle;
            cell.SetCellValue(sqlStr.Substring(i * max, max));
            sheet.AddMergedRegion(new CellRangeAddress(cRowIndex, cRowIndex, 1, 8));
        }

        var extra = sqlStr.Length % max;
        if (extra > 0)
        {
            var cRowIndex = num + sqlStrRowsStartIndex;
            var cell = sheet.GetRow(cRowIndex).GetCell(1);
            cell.CellStyle = bodyStyle;
            cell.SetCellValue(sqlStr.Substring(num * max, extra));
            sheet.AddMergedRegion(new CellRangeAddress(cRowIndex, cRowIndex, 1, 8));
        }

        if (sqlStrRowsTotal > 1)
        {
            sheet.AddMergedRegion(new CellRangeAddress(sqlStrRowsStartIndex, sqlStrRowsTotal + sqlStrRowsStartIndex - 1, 0, 0));
        }

        return nextRowIndexInSheet;
    }

    /// <summary>
    /// 估算需要额外扩展的行数
    /// </summary>
    private int CalCellExtraLine(string sqlStr)
    {
        var maxCellChars = 32767;
        var extraLines = 0;
        if (!string.IsNullOrWhiteSpace(sqlStr))
        {
            var num = sqlStr.Length / maxCellChars;
            extraLines += (num - 1);
            if ((sqlStr.Length % maxCellChars) > 0)
                extraLines++;
        }

        return extraLines;
    }

    /// <summary>
    /// 渲染填充固定列头
    /// </summary>
    private int FillFixColumnValueSection(string schemaName, string schemaCnName, XSSFSheet sheet, int nextRowIndexInSheet,
                                          string remark)
    {
        var secondHeaderRow = sheet.GetRow(nextRowIndexInSheet);

        secondHeaderRow.GetCell(0).SetCellValue("业务域");
        sheet.AddMergedRegion(new CellRangeAddress(nextRowIndexInSheet, nextRowIndexInSheet, 0, 1));

        var secondHeaderRowCell = secondHeaderRow.GetCell(2);
        secondHeaderRowCell.SetCellValue(string.IsNullOrWhiteSpace(schemaCnName) ? schemaName : ($"{schemaName}({schemaCnName})"));
        sheet.AddMergedRegion(new CellRangeAddress(nextRowIndexInSheet, nextRowIndexInSheet, 2, 3));

        secondHeaderRow.GetCell(4).SetCellValue("说明");

        var forthHeaderRowCell = secondHeaderRow.GetCell(5);
        forthHeaderRowCell.SetCellValue($"{remark}");
        sheet.AddMergedRegion(new CellRangeAddress(nextRowIndexInSheet, nextRowIndexInSheet, 5, 8));

        nextRowIndexInSheet++;

        return nextRowIndexInSheet;
    }

    /// <summary>
    /// 填充索引章节
    /// </summary>
    private int FillIndexSection(XSSFSheet sheet, ICellStyle bodyStyle, ICellStyle headerCellStyle, int nextRowIndexInSheet,
                                 int totalColumnSize, SchemaSheetTableExport table)
    {
        if (table.SchemaSheetTableIndexList.Count == 0)
            return nextRowIndexInSheet;

        var indexRowsCount = table.SchemaSheetTableIndexList.Count + 1;

        for (var rh = 0; rh <= indexRowsCount; rh++)
        {
            var indexHeaderRow = sheet.CreateRow(nextRowIndexInSheet + rh);
            for (var c = 0; c < totalColumnSize; ++c)
            {
                indexHeaderRow.CreateCell(c).CellStyle = headerCellStyle;
            }
        }

        var indexHeaders = new List<string>
                           {
                               "索引",
                               "索引类型",
                               "索引名",
                               "索引字段列表",
                               "说明",
                               "是否唯一"
                           };
        var headRowIndex = sheet.GetRow(nextRowIndexInSheet);
        var hIndex = 0;
        foreach (var head in indexHeaders)
        {
            var indexCellHeader = headRowIndex.GetCell(hIndex);
            indexCellHeader.SetCellValue(head);
            if (head == "索引字段列表")
            {
                sheet.AddMergedRegion(new CellRangeAddress(nextRowIndexInSheet, nextRowIndexInSheet, hIndex, hIndex + 1));
                hIndex += 2;
            }
            else if (head == "说明")
            {
                sheet.AddMergedRegion(new CellRangeAddress(nextRowIndexInSheet, nextRowIndexInSheet, hIndex, hIndex + 2));
                hIndex += 3;
            }
            else
                hIndex++;

            indexCellHeader.CellStyle = headerCellStyle;
        }

        nextRowIndexInSheet++;

        var dataTableIndexs = table.SchemaSheetTableIndexList.ToDataTable();
        for (var x = 0; x < dataTableIndexs.Rows.Count; ++x)
        {
            var row = sheet.GetRow(nextRowIndexInSheet);
            var cellIndex = 0;
            for (var j = 0; j < indexHeaders.Count; ++j)
            {
                var cell = row.GetCell(cellIndex);
                var rawStr = dataTableIndexs.Rows[x][j].ToString();
                cell.SetCellValue(rawStr);

                var head = dataTableIndexs.Columns[j].ColumnName;
                if (head == "索引字段列表")
                {
                    sheet.AddMergedRegion(new CellRangeAddress(nextRowIndexInSheet, nextRowIndexInSheet, 3, 5));
                    cellIndex += 3;
                }
                else if (head == "说明")
                {
                    sheet.AddMergedRegion(new CellRangeAddress(nextRowIndexInSheet, nextRowIndexInSheet, 6, 8));
                    cellIndex += 3;
                }
                else
                    cellIndex++;

                cell.CellStyle = bodyStyle;
            }

            nextRowIndexInSheet++;
        }

        return nextRowIndexInSheet;
    }

    /// <summary>
    /// 为指定Cell创建超链接
    /// </summary>
    private void CreateHyperLink(IWorkbook workbook, ICell cell, string address, string cellValue = "")
    {
        if (!string.IsNullOrWhiteSpace(cellValue))
            cell.SetCellValue(cellValue);
        cell.Hyperlink = (new XSSFHyperlink(HyperlinkType.Document) { Address = address });
        cell.CellStyle = GetHyperlinkStyle(workbook);
    }

    /// <summary>
    /// 获取超链接单元格样式
    /// </summary>
    private static ICellStyle GetHyperlinkStyle(IWorkbook workbook)
    {
        var hLinkStyle = workbook.CreateCellStyle();
        var hLinkFont = workbook.CreateFont();
        hLinkFont.Underline = FontUnderlineType.Single;
        hLinkFont.Color = IndexedColors.Blue.Index;
        hLinkStyle.SetFont(hLinkFont);
        return hLinkStyle;
    }

    /// <summary>
    /// 获取表头单元格样式
    /// </summary>
    private static ICellStyle GetHeaderStyle(IWorkbook workbook)
    {
        var headerStyle = workbook.CreateCellStyle();
        headerStyle.FillPattern = FillPattern.SolidForeground;
        headerStyle.FillForegroundColor = IndexedColors.LightTurquoise.Index;

        headerStyle.BorderBottom = BorderStyle.Thin;
        headerStyle.BorderLeft = BorderStyle.Thin;
        headerStyle.BorderRight = BorderStyle.Thin;
        headerStyle.BorderTop = BorderStyle.Thin;
        headerStyle.Alignment = HorizontalAlignment.Center;
        headerStyle.VerticalAlignment = VerticalAlignment.Center;
        var font = workbook.CreateFont();
        font.IsBold = true;
        headerStyle.SetFont(font);
        return headerStyle;
    }

    /// <summary>
    /// 默认单元格样式
    /// </summary>
    private static ICellStyle GetBodyStyle(IWorkbook workbook, short color)
    {
        var cellDefaultStyle = workbook.CreateCellStyle();
        cellDefaultStyle.FillPattern = FillPattern.SolidForeground;
        cellDefaultStyle.FillForegroundColor = color;
        cellDefaultStyle.BorderBottom = BorderStyle.Thin;
        cellDefaultStyle.BorderLeft = BorderStyle.Thin;
        cellDefaultStyle.BorderRight = BorderStyle.Thin;
        cellDefaultStyle.BorderTop = BorderStyle.Thin;
        cellDefaultStyle.Alignment = HorizontalAlignment.Center;
        cellDefaultStyle.VerticalAlignment = VerticalAlignment.Center;
        return cellDefaultStyle;
    }

    /// <summary>
    /// 填充第一行
    /// </summary>
    private int FillFirstRowSection(XSSFSheet sheet, int nextRowIndexInSheet, string structName, string structCnName)
    {
        var tableHeaderCell = sheet.GetRow(nextRowIndexInSheet).GetCell(0);
        tableHeaderCell.SetCellValue(string.IsNullOrWhiteSpace(structCnName) ? structName : ($"{structName}({structCnName})"));
        sheet.AddMergedRegion(new CellRangeAddress(nextRowIndexInSheet, nextRowIndexInSheet, 0, 8));
        nextRowIndexInSheet++;
        return nextRowIndexInSheet;
    }

    /// <summary>
    /// 提前创建当前结构(视图,存储过程)初始行
    /// </summary>
    private void CreateRowAheadOfTime(XSSFSheet sheet, ICellStyle headerCellStyle, int nextRowIndexInSheet, int totalColumnSize)
    {
        for (var rh = 0; rh < 2; rh++)
        {
            var schemaHeaderRow = sheet.CreateRow(nextRowIndexInSheet + rh);
            for (var c = 0; c < totalColumnSize; ++c)
            {
                schemaHeaderRow.CreateCell(c).CellStyle = headerCellStyle;
            }
        }
    }

    /// <summary>
    /// 处理默认值
    /// </summary>
    /// <param name="defaultValue"></param>
    private string HandlerDefaultValue(string defaultValue)
    {
        if (string.IsNullOrEmpty(defaultValue))
            return defaultValue;
        return defaultValue.Replace("::character varying", "").Replace("::text", "");
    }
}