using DbOperateHelper.ConsoleApp.ServerTest.BusyData;

// 一个封装的DbHelper
var dbFaceInfoRecord = new DbFaceInfoRecord();

var count = dbFaceInfoRecord.GetCount();
Console.WriteLine(count);

var dataTable = dbFaceInfoRecord.GetDataTableInfo();

var data = dbFaceInfoRecord.GetInfoWithSerialNumber("111");

Console.ReadLine();