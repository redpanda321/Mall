// ***********************************************************************
// 程序集			: NetRube.Data
// 文件名			: DelBuilder.cs
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
    /// 删除构建器
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class DelBuilder<T> where T : IModel
    {
        private static readonly string ENABLED_FOREIGN_KEY_CHECKS = "SET FOREIGN_KEY_CHECKS=0;";
        private static readonly string DISABLED_FOREIGN_KEY_CHECKS = "SET FOREIGN_KEY_CHECKS=1;";
        private WhereBuilder __where;
        private bool __FOREIGN_KEY_CHECKS = false;
        private BuilderContext context;

        /// <summary>
        /// 初始化一个新 <see cref="DelBuilder&lt;T&gt;" /> 实例。
        /// </summary>
        /// <param name="db">数据库实例</param>
        /// <param name="args">参数</param>
        public DelBuilder(Database db, List<object> args = null)
        {
            context = new BuilderContext(db, args);
        }


        #region FOREIGN_KEY_CHECKS

        /// <summary>
        /// 是否关闭外键检查
        /// </summary>
        /// <param name="flag">true时不执行外键检查</param>
        /// <returns></returns>
        public DelBuilder<T> FOREIGNKEYCHECKS(bool flag = false)
        {
            __FOREIGN_KEY_CHECKS = flag;
            return this;
        }

        #endregion

        #region WHERE
        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>删除构建器</returns>
        public DelBuilder<T> Where(Expression expression)
        {
            if (__where == null)
                __where = new WhereBuilder(context, false);
            __where.Append(expression);
            return this;
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>删除构建器</returns>
        public DelBuilder<T> Where(Expression<Func<T, bool>> expression)
        {
            return this.Where((Expression)expression);
        }

        #endregion

        #region WHERE OR
        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>删除构建器</returns>
        public DelBuilder<T> WhereOr(Expression expression)
        {
            if (__where == null)
                __where = new WhereBuilder(context, false);
            __where.AppendOr(expression);

            return this;
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>删除构建器</returns>
        public DelBuilder<T> WhereOr(Expression<Func<T, bool>> expression)
        {
            return this.WhereOr((Expression)expression);
        }

        #endregion

        #region 内部操作
        private string GetWhere(bool prefix = true)
        {
            if (__where.IsNull()) return string.Empty;
            if (prefix)
                return "WHERE " + __where.ToString();
            return __where.ToString();
        }

        #endregion

        #region 执行
        /// <summary>
        /// 执行操作
        /// </summary>
        /// <returns>受影响的行数</returns>
        public int Execute()
        {
            if (__where == null) return 0;
            var table = context.Database.GetTableName<T>();
            var sql = " {2} DELETE FROM {0} {1} ; {3} ".F(table, this.GetWhere(), __FOREIGN_KEY_CHECKS ? ENABLED_FOREIGN_KEY_CHECKS : "", __FOREIGN_KEY_CHECKS ? DISABLED_FOREIGN_KEY_CHECKS : "");
            return context.Database.Execute(sql, context.Parameters.ToArray());
        }

        /// <summary>
        /// 返回是否执行成功
        /// </summary>
        /// <returns>指示是否执行成功</returns>
        public bool Succeed()
        {
            return this.Execute() > 0;
        }
        #endregion
    }
}