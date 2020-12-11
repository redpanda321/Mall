// ***********************************************************************
// 程序集			: NetRube.Data
// 文件名			: WhereBuilder.cs
// 作者				: NetRube
// 创建时间			: 2013-08-05
//
// 最后修改者		: guozan
// 最后修改时间		: 2017-12-21
// ***********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NetRube.Data
{
    /// <summary>
    /// WHERE 构建器
    /// </summary>
    internal class WhereBuilder : ExpressionVisitor
    {
        private STR __sql;
        public BuilderContext context;
        private Database Database { get { return context.Database; } }
        private bool withTableName;
        private ExpressionType __visitbinarytype = ExpressionType.AndAlso;

        /// <summary>
        /// 初始化一个新 <see cref="WhereBuilder" /> 实例。
        /// </summary>
        /// <param name="context">构建器上下文</param>
        /// <param name="withTableName">指定是否包含表名</param>
        public WhereBuilder(BuilderContext context, bool withTableName = true)
        {
            __sql = new STR();
            this.withTableName = withTableName;
            this.context = context;
        }


        /// <summary>
        /// 返回 WHERE 子句内容
        /// </summary>
        /// <returns>WHERE 子句内容</returns>
        public override string ToString()
        {
            return __sql.ToString();
        }

        #region Append
        private WhereBuilder Append(Expression expression, string type)
        {
            if (expression != null)
            {
                if (!__sql.IsEmpty)
                    __sql.Append(type);
                base.Visit(expression);
            }

            return this;
        }
        
        private WhereBuilder Append(string where, string type)
        {
            if (!where.IsNullOrEmpty_())
            {
                if (!__sql.IsEmpty)
                    __sql.Append(type);
                __sql.Append(where);
            }

            return this;
        }

        /// <summary>添加 WHERE 子句</summary>
        /// <param name="expression">条件表达式</param>
        /// <returns>WHERE 构建器</returns>
        internal WhereBuilder Append(Expression expression)
        {
            __visitbinarytype = ExpressionType.AndAlso;
            return this.Append(expression, " AND ");
        }

        /// <summary>添加 WHERE 子句</summary>
        /// <param name="where">WHERE 子句</param>
        /// <returns>WHERE 构建器</returns>
        internal WhereBuilder Append(string where)
        {
            __visitbinarytype = ExpressionType.AndAlso;
            return this.Append(where, " AND ");
        }

        /// <summary>添加 WHERE OR 子句</summary>
        /// <param name="expression">条件表达式</param>
        /// <returns>WHERE 构建器</returns>
        internal WhereBuilder AppendOr(Expression expression)
        {
            __visitbinarytype = ExpressionType.OrElse;
            return this.Append(expression, " OR ");
        }

        /// <summary>添加 WHERE OR 子句</summary>
        /// <param name="where">WHERE 子句</param>
        /// <returns>WHERE 构建器</returns>
        internal WhereBuilder AppendOr(string where)
        {
            __visitbinarytype = ExpressionType.OrElse;
            return this.Append(where, " OR ");
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
            if (expression.Right is ConstantExpression && (expression.NodeType == ExpressionType.Equal || expression.NodeType == ExpressionType.NotEqual))
            {//X == null or X !=null
                var constant = expression.Right as ConstantExpression;
                MemberExpression member = null;
                if (expression.Left is MemberExpression)
                    member = expression.Left as MemberExpression;
                if (expression.Left is UnaryExpression)
                    if (((UnaryExpression)expression.Left).Operand is MemberExpression)
                        member = ((UnaryExpression)expression.Left).Operand as MemberExpression;

                if (member != null && constant.Value == null)
                {
                    if (expression.NodeType == ExpressionType.Equal)
                        __sql.AppendFormat("( {0} IS NULL )", this.GetColumnName(withTableName, Database, member));
                    else
                        __sql.AppendFormat("( {0} IS NOT NULL )", this.GetColumnName(withTableName, Database, member));
                    return expression;
                }
            }

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
            if (IsParameterOrConstant(expression))
            {
                if (IsDateTimeProperty(expression))
                {
                    ParseDateTimeProperty(expression);
                }
                else
                {
                    __sql.Append(this.GetColumnName(withTableName, Database, expression));
                }
            }
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
                case "Contains":
                    this.ParseLike(expression, "Contains");
                    break;
                case "StartsWith":
                    this.ParseLike(expression, "StartsWith");
                    break;
                case "EndsWith":
                    this.ParseLike(expression, "EndsWith");
                    break;
                case "ExExists":
                    this.ParseExists(expression, false);
                    break;
                case "ExNotExists":
                    this.ParseExists(expression, true);
                    break;
                case "ExIn":
                    this.ParseIn(expression);
                    break;
                case "ExNotIn":
                    this.ParseNotIn(expression);
                    break;
                case "ExIsNull":
                    this.ParseFieldIsNull(expression);
                    break;
                case "ExIsNotNull":
                    this.ParseFieldIsNotNull(expression);
                    break;
                case "Equals":
                    this.ParseEquals(expression);
                    break;
                case "ToString":
                    this.ParseToString(expression);
                    break;
                case "ExIfNull":
                    this.ParseIfNull(expression);
                    break;
                case "ExFormat":
                    this.ParseFormat(expression);
                    break;
                case "ExResolve":
                    this.ParseContinue(expression);
                    break;
                case "ToLower":
                    this.ParseString(expression, "LOWER");
                    break;
                case "ToUpper":
                    this.ParseString(expression, "UPPER");
                    break;
                case "Trim":
                    this.ParseString(expression, "TRIM");
                    break;
                default:
                    throw new NotImplementedException("暂时没实现 “{0}” 方法".F(expression.Method.Name));
            }

            return expression;
        }
        #endregion

        #region 内部处理
        protected void AppendToSql(string strsql)
        {
            __sql.Append(strsql);
        }

        protected virtual void ParseFormat(MethodCallExpression expression)
        {
            __sql.AppendFormat("({0})", GetRightValue(expression.Arguments[0]));
            //this.Visit(expression.Arguments[0]);
        }
        protected virtual void ParseIfNull(MethodCallExpression expression)
        {
            __sql.Append(" IFNULL(");
            this.Visit(expression.Arguments[0]);
            __sql.Append(", ");
            var value = this.GetRightValue(expression.Arguments[1]);
            if (value.IsValue_())
            {
                __sql.Append(value);
            }
            else
            {
                __sql.Append("'");
                __sql.Append(value);
                __sql.Append("'");
            }
            __sql.Append(") ");
        }
        protected virtual void ParseToString(MethodCallExpression expression)
        {
            __sql.AppendFormat("@{0}", context.Parameters.Count);
            var value = this.GetRightValue(expression.Object).ToString();
            context.Parameters.Add(value);
        }
        protected virtual void ParseExists(MethodCallExpression expression, bool isNotExists)
        {
            var value = this.GetRightValue(expression.Arguments[1]);
            if (value is IGetBuilder)
            {
                var val = value as IGetBuilder;
                __sql.AppendFormat("({1} Exists ({0}))", val.GetSql(context.Parameters), isNotExists ? "NOT" : "");
            }
        }
        protected virtual void ParseIn(MethodCallExpression expression)
        {
            var column = expression.Arguments[0] as MemberExpression;
            var value = this.GetRightValue(expression.Arguments[1]);
            if (value is IEnumerable)
            {
                var val = value as IEnumerable;
                if (val.IsNullOrEmpty_())
                {
                    switch (__visitbinarytype)
                    {
                        case ExpressionType.And:
                        case ExpressionType.AndAlso: __sql.Append("(1<>1)"); break;
                        case ExpressionType.Or:
                        case ExpressionType.OrElse: __sql.Append("(1=1)"); break;
                        default: throw new NotImplementedException("暂时没实现 “{0}” 方法".F(__visitbinarytype.GetDescription_()));
                    }
                }
                else
                {
                    __sql.AppendFormat("({0} IN (@{1}))", GetColumnName(withTableName, Database, column), context.Parameters.Count);
                    context.Parameters.Add(val);
                }
            }
            else if (value is IGetBuilder)
            {
                var val = value as IGetBuilder;
                __sql.AppendFormat("({0} IN ({1}))", GetColumnName(withTableName, Database, column), val.GetSql(context.Parameters));
            }
        }
        protected virtual void ParseNotIn(MethodCallExpression expression)
        {
            var column = expression.Arguments[0] as MemberExpression;
            var value = this.GetRightValue(expression.Arguments[1]);
            if (value is IEnumerable)
            {
                var val = value as IEnumerable;
                if (val.IsNullOrEmpty_())
                {
                    switch (__visitbinarytype)
                    {
                        case ExpressionType.And:
                        case ExpressionType.AndAlso: __sql.Append("(1=1)"); break;
                        case ExpressionType.Or:
                        case ExpressionType.OrElse: __sql.Append("(1<>1)"); break;
                        default: throw new NotImplementedException("暂时没实现 “{0}” 方法".F(__visitbinarytype.GetDescription_()));
                    }
                }
                else
                {
                    __sql.AppendFormat("({0} NOT IN (@{1}))", GetColumnName(withTableName, Database, column), context.Parameters.Count);
                    context.Parameters.Add(val);
                }
            }
            else if (value is IGetBuilder)
            {
                var val = value as IGetBuilder;
                __sql.AppendFormat("({0} NOT IN ({1}))", GetColumnName(withTableName, Database, column), val.GetSql(context.Parameters));
            }
            else
            {
                __sql.AppendFormat("(1 <> 1)");
            }
        }
        protected virtual void ParseFieldIsNull(MethodCallExpression expression)
        {
            var column = expression.Arguments[0] as MemberExpression;
            __sql.AppendFormat("( {0} IS NULL )", this.GetColumnName(withTableName, Database, column));
        }
        protected virtual void ParseFieldIsNotNull(MethodCallExpression expression)
        {
            var column = expression.Arguments[0] as MemberExpression;
            __sql.AppendFormat("( {0} IS NOT NULL )", this.GetColumnName(withTableName, Database, column));
        }
        protected virtual void ParseEquals(MethodCallExpression expression)
        {
            var column = expression.Object as MemberExpression;
            var value = this.GetRightValue(expression.Arguments[0]);

            __sql.AppendFormat("( {0} = @{1} )", this.GetColumnName(withTableName, Database, column), context.Parameters.Count);
            context.Parameters.Add(value);
        }

        protected virtual void ParseString(MethodCallExpression expression, string type)
        {
            if (expression.Object is MemberExpression)
            {
                var column = expression.Object as MemberExpression;
                if (IsParameterOrConstant(column))
                {
                    __sql.AppendFormat("{1}({0})", this.GetColumnName(withTableName, Database, column), type);
                    return;
                }
            }

            __sql.Append(type);
            __sql.Append("(");
            this.Visit(expression.Object);
            __sql.Append(")");
        }

        protected virtual void ParseLike(MethodCallExpression expression, string type)
        {
            var left = expression.Object;
            var right = expression.Arguments[0];
            var isLeftParameter = IsParameterOrConstant(left as MemberExpression);
            var value = this.GetRightValue(isLeftParameter ? right : left);
            if (isLeftParameter)
            {
                if (left is MemberExpression)
                    __sql.AppendFormat("({0} LIKE @{1})", this.GetColumnName(withTableName, Database, left as MemberExpression), context.Parameters.Count);
                else
                    __sql.AppendFormat("({0} LIKE @{1})", new StrBuilder(Database, left).ToString(), context.Parameters.Count);
                switch (type)
                {
                    case "Contains":
                        value = "%" + value.ToString().Trim('%') + "%";
                        break;
                    case "StartsWith":
                        value = value.ToString().Trim('%') + "%";
                        break;
                    case "EndsWith":
                        value = "%" + value.ToString().Trim('%');
                        break;
                }
            }
            else
            {
                __sql.AppendFormat("(@{0} Like CONCAT(", context.Parameters.Count);
                if (type == "Contains" || type == "StartsWith") __sql.AppendFormat("'%',");
                __sql.AppendFormat(this.GetColumnName(withTableName, Database, right as MemberExpression));
                if (type == "Contains" || type == "EndsWith") __sql.AppendFormat(",'%'");
                __sql.AppendFormat("))");
            }
            context.Parameters.Add(value);
        }
        protected void ParseContinue(MethodCallExpression expression)
        {
            var value = this.GetRightValue(expression.Arguments[0]);
            if (value is IGetBuilder)
            {
                __sql.AppendFormat(" ({0}) ", (value as IGetBuilder).GetSql(context.Parameters));
            }
        }

        protected virtual void ParseDateTimeProperty(MemberExpression expression)
        {
            __sql.AppendFormat(" {0}(", expression.Member.Name);
            __sql.Append(this.GetColumnName(withTableName, Database, expression.Expression as MemberExpression));
            __sql.Append(") ");
        }
        #endregion
    }
}