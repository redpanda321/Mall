// ***********************************************************************
// 程序集			: NetRube.Data
// 文件名			: GroupByBuilder.cs
// 作者				: guozan
// 创建时间			: 2017-10-16
//
// 最后修改者		: guozan
// 最后修改时间		: 2017-10-16
// ***********************************************************************

using Mall.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace NetRube.Data
{
    /// <summary>
    /// GroupBy 构建器
    /// </summary>
    internal class GroupByBuilder : ExpressionVisitor
    {
        private STR __sql;
        BuilderContext context;
        private bool withTableName;

        /// <summary>
        ///  初始化一个新 <see cref="GroupByBuilder" /> 实例。
        /// </summary>
        /// <param name="context">构建器上下文</param>
        public GroupByBuilder(BuilderContext context)
        {
            __sql = new STR();
            this.context = context;
            this.withTableName = true;
        }

        /// <summary>
        /// 返回 Group By 子句内容
        /// </summary>
        /// <returns>Group By 子句内容</returns>
        public override string ToString()
        {
            return __sql.ToString();
        }

        /// <summary>
        /// 返回 Group By 子句是否为空
        /// </summary>
        /// <returns>返回 Group By 子句是否为空</returns>
        public bool IsEmpty()
        {
            return __sql.IsEmpty;
        }

        #region Append

        /// <summary>添加 Group By 子句</summary>
        /// <param name="expression">条件表达式</param>
        /// <returns>Group By 构建器</returns>
        internal GroupByBuilder Append(Expression expression)
        {
            if (expression != null)
                base.Visit(expression);
            return this;
        }

        #endregion

        #region 重写 ExpressionVisitor
        /// <summary>
        /// 处理二元运算表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>表达式</returns>
        protected override Expression VisitBinary(BinaryExpression expression)
        {
            __sql.Append("(");
            this.Visit(expression.Left);
            __sql.Append(this.GetOperator(expression.NodeType));
            this.Visit(expression.Right);
            __sql.Append(")");

            return expression;
        }

        /// <summary>
        /// 处理字段或属性表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>表达式</returns>
        protected override Expression VisitMemberAccess(MemberExpression expression)
        {
            if (!__sql.IsEmpty)
                __sql.Append(", ");
            if (IsParameterOrConstant(expression))
            {
                if (IsDateTimeProperty(expression))
                    ParseDateTimeProperty(expression);
                else
                    __sql.Append(this.GetColumnName(withTableName, context.Database, expression));
            }
            else
            {
                __sql.AppendFormat("@{0}", context.Parameters.Count);
                context.Parameters.Add(this.GetRightValue(expression));
            }
            return expression;
        }

        #endregion

        #region 内部处理

        private void ParseDateTimeProperty(MemberExpression expression)
        {
            __sql.AppendFormat(" {0}(", expression.Member.Name);
            __sql.Append(this.GetColumnName(withTableName, context.Database, expression.Expression as MemberExpression));
            __sql.Append(")");
        }

        #endregion
    }
}