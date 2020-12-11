using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace NetRube.Data
{
    /// <summary>
    /// 字符串解析
    /// </summary>
    internal class StrBuilder : ExpressionVisitor
    {
        private STR __sql;
        private Database __db;
        private bool __wtn;
        private List<string> __args;
        private int i = 0;

        /// <summary>
        /// 初始化一个新 <see cref="StrBuilder" /> 实例。
        /// </summary>
        /// <param name="db">数据库实例</param>
        /// /// <param name="expression">表达式</param>
        /// <param name="withTableName">指定是否包含表名</param>
        public StrBuilder(Database db, Expression expression, bool withTableName = true)
        {
            __sql = new STR();
            __db = db;
            __wtn = withTableName;
            __args = new List<string>();
            this.Visit(expression);
        }

        /// <summary>
        /// 返回 字符串 内容
        /// </summary>
        /// <returns>返回 字符串 内容</returns>
        public override string ToString()
        {
            __sql.Append("(");
            __sql.Append(__args[0]);
            __sql.Append(")");
            return __sql.ToString();
        }

        #region 重写 ExpressionVisitor

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            var temp = this.GetOperator(expression.NodeType);
            this.Visit(expression.Left);
            this.Visit(expression.Right);
            __args = new List<string> { string.Format(temp, __args[i++], __args[i++]) };
            i = 0;
            return expression;
        }

        protected override Expression VisitConstant(ConstantExpression expression)
        {
            __args.Add("'" + expression.Value + "'");
            return expression;
        }

        protected override Expression VisitMemberAccess(MemberExpression expression)
        {
            if (IsParameterOrConstant(expression))
                __args.Add(this.GetColumnName(__wtn, __db, expression));
            else
            {
                __args.Add(this.GetRightValue(expression) + "");
            }

            return expression;
        }

        #endregion

        protected override string GetOperator(ExpressionType expType)
        {
            switch (expType)
            {
                case ExpressionType.Add:
                    return __db._dbType.EscapeSqlConcat();
                default:
                    throw new NotSupportedException("不支持 “{0}” 操作符".F(expType));
            }
        }
    }
}
