// ***********************************************************************
// 程序集			: NetRube.Data
// 文件名			: UpdateBuilder.cs
// 作者				: NetRube
// 创建时间			: 2013-08-05
//
// 最后修改者		: NetRube
// 最后修改时间		: 2013-11-05
// ***********************************************************************

using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NetRube.Data
{
    /// <summary>
    /// UPDATE 构建器
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    internal class UpdateBuilder<T> : ExpressionVisitor where T : IModel
    {
        private BuilderContext context;
        private STR __sql;
        private Database Database { get { return context.Database; } }


        public UpdateBuilder(BuilderContext context)
        {
            __sql = new STR();
            this.context = context;
        }


        /// <summary>
        /// 返回 SET 子句内容
        /// </summary>
        /// <returns>SET 子句内容</returns>
        public override string ToString()
        {
            if (__sql.IsEmpty)
                return string.Empty;
            return __sql.ToString();
        }

        #region Append
        /// <summary>添加 SET 子句</summary>
        /// <param name="field">SET 字段</param>
        /// <param name="value">SET 值</param>
        /// <returns>UPDATE 构建器</returns>
        internal UpdateBuilder<T> Append(Expression<Func<T, dynamic>> field, dynamic value)
        {
            return this.InternalAppend(GetColumnName(false, Database, Utils.GetMemberExpression(field)), value);
        }

        /// <summary>添加 SET 子句</summary>
        /// <param name="field">SET 字段</param>
        /// <param name="value">SET 值</param>
        /// <returns>UPDATE 构建器</returns>
        internal UpdateBuilder<T> Append(string field, dynamic value)
        {
            return this.InternalAppend(Database.GetColumnName(field), value);
        }

        /// <summary>添加 SET 子句</summary>
        /// <param name="field">SET 字段</param>
        /// <param name="value">SET 值</param>
        /// <returns>UPDATE 构建器</returns>
        internal UpdateBuilder<T> Append(Expression<Func<T, dynamic>> field, Expression value)
        {
            return this.InternalAppend(GetColumnName(false, Database, Utils.GetMemberExpression(field)), value);
        }

        /// <summary>添加 SET 子句</summary>
        /// <param name="field">SET 字段</param>
        /// <param name="value">SET 值</param>
        /// <returns>UPDATE 构建器</returns>
        internal UpdateBuilder<T> Append(string field, Expression value)
        {
            return this.InternalAppend(Database.GetColumnName(field), value);
        }

        private UpdateBuilder<T> InternalAppend(string field, Expression value)
        {
            if (!__sql.IsEmpty)
                __sql.Append(", ");
            __sql.AppendFormat("{0} = ", field);
            base.Visit(value);

            return this;
        }
        private UpdateBuilder<T> InternalAppend(string field, dynamic value)
        {
            if (!__sql.IsEmpty)
                __sql.Append(", ");
            __sql.AppendFormat("{0} = @{1}", field, context.Parameters.Count);
            context.Parameters.Add(value);

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
            this.Visit(expression.Left);
            __sql.Append(this.GetOperator(expression.NodeType));
            this.Visit(expression.Right);

            return expression;
        }

        /// <summary>
        /// 处理字段或属性表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>表达式</returns>
        protected override Expression VisitMemberAccess(MemberExpression expression)
        {
            if (IsParameterOrConstant(expression))
                __sql.Append(this.GetColumnName(true, Database, expression));
            else
            {
                __sql.AppendFormat("@{0}", context.Parameters.Count);
                context.Parameters.Add(this.GetRightValue(expression));
            }

            return expression;
        }

        /// <summary>
        /// 处理常量表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>表达式</returns>
        protected override Expression VisitConstant(ConstantExpression expression)
        {
            __sql.AppendFormat("@{0}", context.Parameters.Count);
            context.Parameters.Add(expression.Value);
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
                case "ToString":
                    this.ParseToString(expression);
                    break;
                default:
                    throw new NotImplementedException("暂时没实现 “{0}” 方法".F(expression.Method.Name));
            }

            return expression;
        }

        private void ParseToString(MethodCallExpression expression)
        {
            __sql.AppendFormat("@{0}", context.Parameters.Count);
            var value = this.GetRightValue(expression.Object).ToString();
            context.Parameters.Add(value);
        }
        #endregion
    }
}