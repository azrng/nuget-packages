
namespace PostgresSqlSample.Model
{
    public class Test1 : IdentityBaseEntity<string>
    {
        public long LongTest { get; set; }

        public double DoubleTest { get; set; }

        public decimal DecimalTest { get; set; }

        public float FloatTest { get; set; }
    }

    public class Test2 : IdentityOperatorEntity<string> { }

    public class Test3 : IdentityOperatorStatusEntity<string> { }

    public class Task1Etc : EntityTypeConfigurationIdentity<Test1, string> { }

    public class Task2Etc : EntityTypeConfigurationIdentityOperator<Test2, string> { }

    public class Task3Etc : EntityTypeConfigurationIdentityOperatorStatus<Test3, string> { }
}