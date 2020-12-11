// ***********************************************************************
// 程序集			: NetRube.Data
// 文件名			: HavingBuilder.cs
// 作者				: guozan
// 创建时间			: 2017-12-21
//
// 最后修改者		: guozan
// 最后修改时间		: 2017-12-21
// ***********************************************************************

using Mall.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NetRube.Data
{
    /// <summary>
    /// Having 构建器
    /// </summary>
    internal class HavingBuilder : WhereBuilder
    {
        /// <summary>
        /// 初始化一个新 <see cref="HavingBuilder" /> 实例。
        /// </summary>
        /// <param name="context">构建器上下文</param>
        /// <param name="withTableName">指定是否包含表名</param>
        public HavingBuilder(BuilderContext context, bool withTableName = true)
            : base(context, withTableName)
        {

        }

        #region 重写 ExpressionVisitor

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
                    this.ParseHaving(expression, "SUM");
                    break;
                case "ExAvg":
                    this.ParseHaving(expression, "AVG");
                    break;
                case "ExMax":
                    this.ParseHaving(expression, "MAX");
                    break;
                case "ExMin":
                    this.ParseHaving(expression, "MIN");
                    break;
                default:
                    base.VisitMethodCall(expression); break;
            }

            return expression;
        }
        #endregion

        protected virtual void ParseCount(MethodCallExpression expression)
        {
            var value = this.GetRightValue(expression.Arguments[1]);
            var flag = (value as bool?).Value;
            if (flag)
            {
                AppendToSql(" COUNT(");
                this.Visit(expression.Arguments[0]);
                AppendToSql(") ");
            }
            else
                AppendToSql(" COUNT(0) ");
        }

        protected virtual void ParseHaving(MethodCallExpression expression, string type)
        {
            AppendToSql(string.Format(" {0}(", type));
            this.Visit(expression.Arguments[0]);
            AppendToSql(") ");
        }
    }
}