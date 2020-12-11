// ***********************************************************************
// 程序集			: NetRube.Data
// 文件名			: SelectBuilder.cs
// 作者				: guozan
// 创建时间			: 2017-12-19
//
// 最后修改者		: guozan
// 最后修改时间		: 2017-12-19
// ***********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace NetRube.Data
{
    /// <summary>
    /// Select 构建器
    /// </summary>
    internal class SelectBuilder : ExpressionVisitor
    {
        private STR select;
        private bool whitTableName;
        private BuilderContext context;

        private bool __needdelimiter = true;
        private string __rownoname = string.Empty;
        private bool __isrowno = false;

        /// <summary>
        /// 初始化一个新 <see cref="SelectBuilder" /> 实例。
        /// </summary>
        public SelectBuilder(BuilderContext context, Expression expression)
        {
            this.select = new STR();
            this.context = context;
            this.whitTableName = true;
            if (expression != null)
                base.Visit(expression);
        }


        /// <summary>
        /// 返回 Select 字段
        /// </summary>
        /// <returns>Select 字段</returns>
        public override string ToString()
        {
            return select.ToString();
        }

        /// <summary>
        /// 返回 Select 字段是否为空
        /// </summary>
        /// <returns>Select 字段是否为空</returns>
        public bool IsEmpty()
        {
            return select.IsEmpty;
        }

        /// <summary>
        /// 序列号字段名
        /// </summary>
        public string RowNoName { get { return __rownoname; } }

        #region 重写 ExpressionVisitor

        /// <summary>
        /// 处理构造函数调用表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        protected override Expression VisitNew(NewExpression expression)
        {
            var members = expression.Members;
            var arguments = expression.Arguments;
            var len = members.Count < arguments.Count ? members.Count : arguments.Count;
            for (int i = 0; i < len; i++)
            {
                this.Visit(arguments[i]);
                if (__isrowno)
                {
                    __rownoname = members[i].Name;
                    __isrowno = false;
                    continue;
                }
                if (arguments[i].NodeType == ExpressionType.MemberAccess && (arguments[i] as MemberExpression).Member.Name == members[i].Name) continue;
                select.Append(context.Database._dbType.EscapeNewName(members[i].Name));
            }
            return expression;
        }

        /// <summary>
        /// 处理方法调用表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>表达式</returns>
        /// <exception cref="System.NotImplementedException">指定方法未实现</exception>
        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            switch (expression.Method.Name)
            {
                case "ExCount":
                    this.ParseCount(expression);
                    break;
                case "ExSum":
                    this.ParseSum(expression);
                    break;
                case "ExAvg":
                    this.ParseAvg(expression);
                    break;
                case "ExMax":
                    this.ParseMax(expression);
                    break;
                case "ExMin":
                    this.ParseMin(expression);
                    break;
                case "ExIfNull":
                    this.ParseIfNull(expression);
                    break;
                case "ExExists":
                    this.ParseExists(expression);
                    break;
                case "ExResolve":
                    this.ParseContinue(expression, 0);
                    break;
                case "ExCase":
                    this.ParseCase(expression);
                    break;
                case "ExThen":
                    this.ParseThen(expression);
                    break;
                case "ExRowNo":
                    this.ParseRowNo(expression);
                    break;
                case "ExSql":
                    this.ParseNativeSQL(expression);
                    break;
                default:
                    throw new NotImplementedException("暂时没实现 “{0}” 方法".F(expression.Method.Name));
            }

            return expression;
        }

        /// <summary>
        /// 处理字段或属性表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>表达式</returns>
        protected override Expression VisitMemberAccess(MemberExpression expression)
        {
            if (!select.IsEmpty && __needdelimiter)
                select.Append(", ");
            if (IsParameterOrConstant(expression))
            {
                if (IsDateTimeProperty(expression))
                {
                    ParseDateTimeProperty(expression);
                }
                else
                {
                    select.Append(this.GetColumnName(whitTableName, context.Database, expression));
                }
            }
            else
            {
                var value = this.GetRightValue(expression);
                if (value is IGetBuilder)
                {
                    select.Append((value as IGetBuilder).GetSql(context.Parameters));
                }
                else
                {
                    if (value.IsValue_())
                    {
                        select.Append(value);
                    }
                    else
                    {
                        select.AppendFormat("'{0}'", value);
                    }
                }
            }

            return expression;
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            var addAndSub = expression.NodeType == ExpressionType.Add || expression.NodeType == ExpressionType.Subtract;
            var temp = __needdelimiter;
            if (addAndSub)
            {
                if (!select.IsEmpty && __needdelimiter)
                    select.Append(", ");
                select.Append("(");
                __needdelimiter = false;
            }
            this.Visit(expression.Left);
            if (addAndSub) __needdelimiter = temp;

            if (expression.Right.NodeType != ExpressionType.Call || (expression.Right as MethodCallExpression).Method.Name != "ExThen")
                select.Append(this.GetOperator(expression.NodeType));
            temp = __needdelimiter;
            __needdelimiter = false;
            this.Visit(expression.Right);
            if (addAndSub) select.Append(")");
            __needdelimiter = temp;
            return expression;
        }

        protected override Expression VisitConstant(ConstantExpression expression)
        {
            if (!select.IsEmpty && __needdelimiter)
                select.Append(", ");
            var value = expression.Value;
            if (value.IsValue_())
                select.Append(value);
            else
                select.AppendFormat("'{0}'", value);
            return expression;
        }
        #endregion

        #region 内部处理

        private void ParseSum(MethodCallExpression expression)
        {
            if (!select.IsEmpty && __needdelimiter)
                select.Append(", ");
            select.Append(" SUM(");
            var temp = __needdelimiter;
            __needdelimiter = false;
            this.Visit(expression.Arguments[0]);
            __needdelimiter = temp;
            select.Append(") ");
        }

        private void ParseCount(MethodCallExpression expression)
        {
            if (!select.IsEmpty && __needdelimiter)
                select.Append(", ");
            select.Append(" COUNT(0) ");
        }

        private void ParseAvg(MethodCallExpression expression)
        {
            if (!select.IsEmpty && __needdelimiter)
                select.Append(", ");
            select.Append(" Avg(");
            var temp = __needdelimiter;
            __needdelimiter = false;
            this.Visit(expression.Arguments[0]);
            __needdelimiter = temp;
            select.Append(") ");
        }

        private void ParseMax(MethodCallExpression expression)
        {
            if (!select.IsEmpty && __needdelimiter)
                select.Append(", ");
            select.Append(" MAX(");
            var temp = __needdelimiter;
            __needdelimiter = false;
            this.Visit(expression.Arguments[0]);
            __needdelimiter = temp;
            select.Append(") ");
        }

        private void ParseMin(MethodCallExpression expression)
        {
            if (!select.IsEmpty && __needdelimiter)
                select.Append(", ");
            select.Append(" MIN(");
            var temp = __needdelimiter;
            __needdelimiter = false;
            this.Visit(expression.Arguments[0]);
            __needdelimiter = temp;
            select.Append(") ");
        }

        private void ParseContinue(MethodCallExpression expression, int index)
        {
            var value = this.GetRightValue(expression.Arguments[index]);
            if (value is IGetBuilder)
            {
                if (!select.IsEmpty && __needdelimiter)
                    select.Append(", ");
                select.Append(" (" + (value as IGetBuilder).GetSql(context.Parameters) + ") ");
            }
        }

        private void ParseExists(MethodCallExpression expression)
        {
            if (!select.IsEmpty && __needdelimiter)
                select.Append(", ");
            select.Append("(EXISTS ");
            var temp = __needdelimiter;
            __needdelimiter = false;
            this.ParseContinue(expression, 1);
            __needdelimiter = temp;
            select.Append(")");
        }

        private void ParseIfNull(MethodCallExpression expression)
        {
            if (!select.IsEmpty && __needdelimiter)
                select.Append(", ");
            select.Append(" IFNULL(");
            var temp = __needdelimiter;
            __needdelimiter = false;
            this.Visit(expression.Arguments[0]);
            __needdelimiter = temp;
            select.Append(", ");
            var value = this.GetRightValue(expression.Arguments[1]);
            if (value.IsValue_())
            {
                select.Append(value);
            }
            else
            {
                select.Append("'");
                select.Append(value);
                select.Append("'");
            }
            select.Append(") ");
        }

        private void ParseRowNo(MethodCallExpression expression)
        {
            __isrowno = true;
            __rownoname = "RowNo";
        }

        private void ParseDateTimeProperty(MemberExpression expression)
        {
            select.AppendFormat(" {0}(", expression.Member.Name);
            select.Append(this.GetColumnName(whitTableName, context.Database, expression.Expression as MemberExpression));
            select.AppendFormat(") {0}", context.Database._dbType.EscapeNewName(expression.Member.Name));
        }

        private void ParseCase(MethodCallExpression expression)
        {
            if (!select.IsEmpty && __needdelimiter)
                select.Append(", ");
            var temp = __needdelimiter;
            __needdelimiter = false;
            select.Append("( CASE ");
            var value = this.GetRightValue(expression.Arguments[2]);
            foreach (var e in value as IEnumerable)
            {
                select.Append(" WHEN ");
                this.Visit((Expression)e);
            }
            __needdelimiter = temp;
            select.Append(" ELSE ");
            var defaultvalue = this.GetRightValue(expression.Arguments[1]);
            if (defaultvalue.IsValue_())
                select.Append(defaultvalue);
            else
                select.AppendFormat("'{0}'", defaultvalue);
            select.Append(" END) ");
        }
        private void ParseNativeSQL(MethodCallExpression expression)
        {
            if (!select.IsEmpty && __needdelimiter)
                select.Append(", ");
            var arg = expression.Arguments[0];
            var value = this.GetRightValue(arg);
            select.Append(value);
        }
        private void ParseThen(MethodCallExpression expression)
        {
            select.Append(" THEN ");
            var value = this.GetRightValue(expression.Arguments[1]);
            if (value.IsValue_())
                select.Append(value);
            else
                select.AppendFormat("'{0}'", value);
        }

        #endregion
    }
}