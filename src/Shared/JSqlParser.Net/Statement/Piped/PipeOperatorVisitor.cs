namespace JSqlParser.Net.Statement.Piped;

public interface PipeOperatorVisitor<T, S>
{
    T Visit(AggregatePipeOperator aggregate, S context);
    T Visit(AsPipeOperator asOp, S context);
    T Visit(CallPipeOperator call, S context);
    T Visit(DropPipeOperator drop, S context);
    T Visit(ExtendPipeOperator extend, S context);
    T Visit(JoinPipeOperator join, S context);
    T Visit(LimitPipeOperator limit, S context);
    T Visit(OrderByPipeOperator orderBy, S context);
    T Visit(PivotPipeOperator pivot, S context);
    T Visit(RenamePipeOperator rename, S context);
    T Visit(SelectPipeOperator select, S context);
    T Visit(SetPipeOperator setOp, S context);
    T Visit(TableSamplePipeOperator tableSample, S context);
    T Visit(SetOperationPipeOperator union, S context);
    T Visit(UnPivotPipeOperator unPivot, S context);
    T Visit(WherePipeOperator where, S context);
    T Visit(WindowPipeOperator window, S context);
}
