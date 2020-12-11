// ***********************************************************************
// 程序集			: NetRube.Data
// 文件名			: PetaPocoExt.cs
// 作者				: NetRube
// 创建时间			: 2013-08-05
//
// 最后修改者		: NetRube
// 最后修改时间		: 2013-11-07
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NetRube.Data.Internal;
using MongoDB.Driver;
using Mall.Entities;

namespace NetRube.Data
{
    /// <summary>
    /// PetaPoco 扩展
    /// </summary>
    public static class PetaPocoExt
    {
        #region Get
        /// <summary>
        /// 获取实体数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <returns>查询构建器</returns>
        public static GetBuilder<T> Get<T>(this Database db) where T : IModel
        {
            return new GetBuilder<T>(db);
        }

        /// <summary>
        /// 获取实体数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <param name="expression">查询条件表达式</param>
        /// <returns>查询构建器</returns>
        public static GetBuilder<T> Get<T>(this Database db, Expression<Func<T, bool>> expression) where T : IModel
        {
            return new GetBuilder<T>(db).Where(expression);
        }
       

        #endregion

        #region Del
        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <param name="expression">删除条件表达式</param>
        /// <returns>受影响的行</returns>
        public static int Del<T>(this Database db, Expression<Func<T, bool>> expression) where T : IModel
        {
            return new DelBuilder<T>(db).Where(expression).Execute();
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <param name="entity">要删除的实体</param>
        /// <returns>指示是否删除成功</returns>
        public static bool Del<T>(this Database db, T entity) where T : IModel
        {
            return db.Delete(entity) > 0;
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <param name="entitys">要删除的实体集合</param>
        /// <returns>指示是否删除成功</returns>
        public static bool Del<T>(this Database db, IEnumerable<T> entitys) where T : IModel
        {
            return DbFactory.Default
                .InTransaction(() =>
                {
                    var flag = true;
                    foreach (var target in entitys)
                    {
                        flag = db.Delete(target) > 0;
                        if (!flag) return flag;
                    }
                    return flag;
                });
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <returns>删除构建器</returns>
        public static DelBuilder<T> Del<T>(this Database db) where T : IModel
        {
            return new DelBuilder<T>(db);
        }
        #endregion

        #region Set
        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <returns>更新构建器</returns>
        public static SetBuilder<T> Set<T>(this Database db) where T : IModel
        {
            return new SetBuilder<T>(db);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <param name="entity">要更新的实体</param>
        /// <returns>更新构建器</returns>
        public static bool Set<T>(this Database db, T entity) where T : IModel
        {
            return db.Update(entity) > 0;
        }
        #endregion

        #region Add
        /// <summary>
        /// 添加
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <param name="entity">要添加的实体</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool Add<T>(this Database db, T entity) where T : IModel
        {
            var model = entity as IModel;
            var pd = PocoData.ForType(entity.GetType());
            var ret = db.Insert(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, pd.TableInfo.AutoIncrement, entity).ToBool_();
            if (null != model) model.ModifiedComplete();
            return ret;
        }

        /// <summary>
        /// 批量添加
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <param name="entity">要添加的实体集合</param>
        /// <returns>指示是否添加成功</returns>
        public static bool Add<T>(this Database db, IEnumerable<T> entity) where T : IModel
        {
            return AddRange(db, entity);
        }
        public static bool AddRange<T>(this Database db, IEnumerable<T> list) where T : IModel
        {
            if (list == null || list.Count() == 0) return false;
            return DbFactory.Default
                .InTransaction(() =>
                {
                    var flag = true;
                    var pd = PocoData.ForType(list.First().GetType());
                    foreach (var target in list)
                    {
                        var model = target as IModel;
                        flag = db.Insert(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, pd.TableInfo.AutoIncrement, model).ToBool_();
                        model.ModifiedComplete();
                        if (!flag) return flag;
                    }
                    return flag;
                });
        }
        #endregion

            #region 事务
            /// <summary>
            /// 在事务范围内处理
            /// </summary>
            /// <param name="db">数据库实例</param>
            /// <param name="action">要处理操作</param>
            /// <param name="succedAction">The succed action.</param>
            /// <param name="failedAction">处理失败时的操作</param>
            /// <returns>提示是否操作成功</returns>
        public static bool InTransaction(this Database db, Action action, Action succedAction = null, Action<Exception> failedAction = null)
        {
            var r = false;
            db.BeginTransaction();
            try
            {
                action();
                db.CompleteTransaction();
                r = true;
                if (succedAction != null)
                    succedAction();
            }
            catch (Exception ex)
            {
                Mall.Core.Log.Error(ex.Message, ex);
                db.AbortTransaction();
                r = false;
                if (failedAction != null)
                    failedAction(ex);
                else
                    //throw ex;
                    return false;
            }

            return r;
        }

        /// <summary>
        /// 在事务范围内处理
        /// </summary>
        /// <param name="db">数据库实例</param>
        /// <param name="action">要处理操作</param>
        /// <param name="succeedAction">处理成功时的操作</param>
        /// <param name="failedAction">处理失败时的操作</param>
        /// <returns>提示是否操作成功</returns>
        public static bool InTransaction(this Database db, Func<bool> action, Action succeedAction = null, Action<Exception> failedAction = null)
        {
            var r = false;
            db.BeginTransaction();
            try
            {
                if (action())
                {
                    db.CompleteTransaction();
                    r = true;
                    if (succeedAction != null)
                        succeedAction();
                }
                else
                {
                    db.AbortTransaction();
                    r = false;
                    if (failedAction != null)
                        failedAction(new Exception(""));
                }
            }
            catch (Exception ex)
            {
                Mall.Core.Log.Error(ex.Message, ex);
                db.AbortTransaction();
                r = false;
                if (failedAction != null)
                    failedAction(ex);
                else
                    throw ex;
            }

            return r;
        }
        #endregion

        #region 获取表名、列名
        /// <summary>
        /// 获取字段列名称
        /// </summary>
        /// <param name="db">数据库实例</param>
        /// <param name="columnName">原始名称</param>
        /// <returns>转码后的字段列名称</returns>
        /// <exception cref="NetRube.ArgumentNullOrEmptyException"><paramref name="columnName" /> 为空或 null</exception>
        public static string GetColumnName(this Database db, string columnName)
        {
            if (columnName.IsNullOrEmpty_())
                throw new ArgumentNullOrEmptyException("columnName");

            if (columnName == "*") return columnName;
            if (columnName.Contains('.')) return columnName;
            return db._dbType.EscapeSqlIdentifier(columnName);
        }

        /// <summary>
        /// 获取字段列名称
        /// </summary>
        /// <param name="db">数据库实例</param>
        /// <param name="type">实体类型</param>
        /// <param name="columnName">原始名称</param>
        /// <returns>转码后的字段列名称</returns>
        /// <exception cref="NetRube.ArgumentNullOrEmptyException"><paramref name="columnName" /> 为空或 null</exception>
        /// <exception cref="System.NullReferenceException">找不到字段列名</exception>
        public static string GetColumnName(this Database db, Type type, string columnName)
        {
            if (columnName.IsNullOrEmpty_())
                throw new ArgumentNullOrEmptyException("columnName");

            if (columnName.Contains('.')) return columnName;

            var cols = PocoData.ForType(type).Columns;
            if (!cols.ContainsKey(columnName))
            {
                var c = cols.Values.SingleOrDefault(p => p.PropertyInfo.Name == columnName);
                if (c == null)
                    throw new NullReferenceException("在实体 {0} 中没有找到名为“{1}”的字段列名".F(type.Name, columnName));
                columnName = c.ColumnName;
            }

            return GetColumnName(db, columnName);
        }

        /// <summary>
        /// 获取表名称
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <param name="columnName">原字段列名称</param>
        /// <returns>转码后的字段列名称</returns>
        public static string GetColumnName<T>(this Database db, string columnName)
        {
            return GetColumnName(db, typeof(T), columnName);
        }

        /// <summary>
        /// 获取表名称
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <param name="entity">实体</param>
        /// <param name="columnName">原字段列名称</param>
        /// <returns>转码后的字段列名称</returns>
        public static string GetColumnName<T>(this Database db, T entity, string columnName)
        {
            return GetColumnName(db, typeof(T), columnName);
        }


        /// <summary>
        /// 获取表名称
        /// </summary>
        /// <param name="db">数据库实例</param>
        /// <param name="tableName">原始表名</param>
        /// <returns>转码后的表名</returns>
        /// <exception cref="NetRube.ArgumentNullOrEmptyException"><paramref name="tableName" /> 为空或 null</exception>
        public static string GetTableName(this Database db, string tableName)
        {
            if (tableName.IsNullOrEmpty_())
                throw new ArgumentNullOrEmptyException("tableName");

            return db._dbType.EscapeTableName(tableName);
        }

        /// <summary>
        /// 获取表名称
        /// </summary>
        /// <param name="db">数据库实例</param>
        /// <param name="type">实体类型</param>
        /// <returns>转码后的表名</returns>
        public static string GetTableName(this Database db, Type type)
        {
            return GetTableName(db, PocoData.ForType(type).TableInfo.TableName);
        }

        /// <summary>
        /// 获取表名称
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <returns>转码后的表名</returns>
        public static string GetTableName<T>(this Database db)
        {
            return GetTableName(db, typeof(T));
        }

        /// <summary>
        /// 获取表名称
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <param name="entity">实体</param>
        /// <returns>转码后的表名</returns>
        public static string GetTableName<T>(this Database db, T entity)
        {
            return GetTableName(db, typeof(T));
        }

        ///// <summary>
        ///// 按字段表达式获取表名称
        ///// </summary>
        ///// <param name="db">数据库实例</param>
        ///// <param name="expression">字段表达式</param>
        ///// <returns>转码后的表名</returns>
        //public static string GetTableName(this Database db, Expression expression)
        //{
        //    return GetTableName(db, Utils.GetPropertyInfo(expression).ReflectedType);
        //}

        ///// <summary>
        ///// 按字段表达式获取表名称
        ///// </summary>
        ///// <typeparam name="T">实体类型</typeparam>
        ///// <param name="db">数据库实例</param>
        ///// <param name="expression">字段表达式</param>
        ///// <returns>转码后的表名</returns>
        //public static string GetTableName<T>(this Database db, Expression expression)
        //{
        //    return GetTableName(db, typeof(T));
        //}

        ///// <summary>
        ///// 按字段表达式获取表名称
        ///// </summary>
        ///// <typeparam name="T">实体类型</typeparam>
        ///// <param name="db">数据库实例</param>
        ///// <param name="expression">字段表达式</param>
        ///// <returns>转码后的表名</returns>
        //public static string GetTableName<T>(this Database db, Expression<Func<T, object>> expression)
        //{
        //    return GetTableName(db, typeof(T));
        //}

        /// <summary>
        /// 获取表名称和字段列名称
        /// </summary>
        /// <param name="db">数据库实例</param>
        /// <param name="tableName">表名称</param>
        /// <param name="columnName">字段列名称</param>
        /// <returns>组合后的表名称和字段列名称</returns>
        /// <exception cref="NetRube.ArgumentNullOrEmptyException"><paramref name="tableName" /> 或 <paramref name="columnName" /> 为空或 null</exception>
        public static string GetTableAndColumnName(this Database db, string tableName, string columnName)
        {
            if (tableName.IsNullOrEmpty_())
                throw new ArgumentNullOrEmptyException("tableName");
            if (columnName.IsNullOrEmpty_())
                throw new ArgumentNullOrEmptyException("columnName");

            if (columnName.Contains('.')) return columnName;
            return GetTableName(db, tableName) + "." + GetColumnName(db, columnName);
        }

        /// <summary>
        /// 获取表名称和字段列名称
        /// </summary>
        /// <param name="db">数据库实例</param>
        /// <param name="type">实体类型</param>
        /// <returns>组合后的表名称和字段列名称</returns>
        public static string GetTableAndColumnName(this Database db, Type type)
        {
            return GetTableAndColumnName(db, GetTableName(db, type), "*");
        }

        /// <summary>
        /// 获取表名称和字段列名称
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <returns>组合后的表名称和字段列名称</returns>
        public static string GetTableAndColumnName<T>(this Database db)
        {
            return GetTableAndColumnName(db, GetTableName(db, typeof(T)), "*");
        }

        /// <summary>
        /// 获取表名称和字段列名称
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <param name="entity">实体</param>
        /// <returns>组合后的表名称和字段列名称</returns>
        public static string GetTableAndColumnName<T>(this Database db, T entity)
        {
            return GetTableAndColumnName(db, GetTableName(db, typeof(T)), "*");
        }

        /// <summary>
        /// 获取表名称和字段列名称
        /// </summary>
        /// <param name="db">数据库实例</param>
        /// <param name="type">实体类型</param>
        /// <param name="columnName">原字段列名称</param>
        /// <returns>组合后的表名称和字段列名称</returns>
        /// <exception cref="NetRube.ArgumentNullOrEmptyException"><paramref name="columnName" /> 为空或 null</exception>
        /// <exception cref="System.NullReferenceException">找不到字段列名</exception>
        public static string GetTableAndColumnName(this Database db, Type type, string columnName)
        {
            if (columnName.IsNullOrEmpty_())
                throw new ArgumentNullOrEmptyException("columnName");

            if (columnName.Contains('.')) return columnName;

            var pd = PocoData.ForType(type);
            var cols = pd.Columns;
            if (!cols.ContainsKey(columnName))
            {
                var c = cols.Values.SingleOrDefault(p => p.PropertyInfo.Name == columnName);
                if (c == null)
                    throw new NullReferenceException("在实体 {0} 中没有找到名为“{1}”的字段列名".F(type.Name, columnName));
                columnName = c.ColumnName;
            }

            return GetTableAndColumnName(db, pd.TableInfo.TableName, columnName);
        }

        /// <summary>
        /// 获取表名称和字段列名称
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <param name="columnName">原字段列名称</param>
        /// <returns>组合后的表名称和字段列名称</returns>
        public static string GetTableAndColumnName<T>(this Database db, string columnName)
        {
            return GetTableAndColumnName(db, typeof(T), columnName);
        }

        /// <summary>
        /// 获取表名称和字段列名称
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库实例</param>
        /// <param name="entity">实体</param>
        /// <param name="columnName">原字段列名称</param>
        /// <returns>组合后的表名称和字段列名称</returns>
        public static string GetTableAndColumnName<T>(this Database db, T entity, string columnName)
        {
            return GetTableAndColumnName(db, typeof(T), columnName);
        }

        ///// <summary>
        ///// 获取表名称和字段列名称
        ///// </summary>
        ///// <param name="db">数据库实例</param>
        ///// <param name="expression">字段表达式</param>
        ///// <returns>组合后的表名称和字段列名称</returns>
        //public static string GetTableAndColumnName(this Database db, Expression expression)
        //{
        //    var pi = Utils.GetPropertyInfo(expression);
        //    return GetTableAndColumnName(db, pi.ReflectedType, pi.Name);
        //}

        ///// <summary>
        ///// 获取表名称和字段列名称
        ///// </summary>
        ///// <typeparam name="T">实体类型</typeparam>
        ///// <param name="db">数据库实例</param>
        ///// <param name="expression">字段表达式</param>
        ///// <returns>组合后的表名称和字段列名称</returns>
        //public static string GetTableAndColumnName<T>(this Database db, Expression expression)
        //{
        //    return GetTableAndColumnName(db, typeof(T), Utils.GetPropertyName(expression));
        //}

        ///// <summary>
        ///// 获取表名称和字段列名称
        ///// </summary>
        ///// <typeparam name="T">实体类型</typeparam>
        ///// <param name="db">数据库实例</param>
        ///// <param name="expression">字段表达式</param>
        ///// <returns>组合后的表名称和字段列名称</returns>
        //public static string GetTableAndColumnName<T>(this Database db, Expression<Func<T, object>> expression)
        //{
        //    return GetTableAndColumnName(db, typeof(T), Utils.GetPropertyName(expression));
        //}
        #endregion

        ///// <summary>
        ///// 获取数据
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="db"></param>
        ///// <param name="filter"></param>
        ///// <param name="options"></param>
        ///// <returns></returns>
        //public static IFindFluent<T, T> ToList<T>(this IMongoCollection<T> db, Expression<Func<T, bool>> filter, FindOptions options = null)
        //{
        //    return db.Find(filter, options);
        //}

        #region 扩展方法

        /// <summary>
        /// 调用 SQL NOT EXISTS 操作符(Where)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="Table"></typeparam>
        /// <param name="field">字段栏</param>
        /// <param name="gb"></param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static bool ExNotExists<T, Table>(this T field, GetBuilder<Table> gb)
            where T : IModel
            where Table : IModel
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }

        /// <summary>
        /// 调用 SQL EXISTS 操作符(Where)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="Table"></typeparam>
        /// <param name="field">字段栏</param>
        /// <param name="gb"></param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static bool ExExists<T, Table>(this T field, GetBuilder<Table> gb)
            where T : IModel
            where Table : IModel
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }

        /// <summary>
        /// 调用 SQL IN 操作符(Where)
        /// </summary>
        /// <typeparam name="T">字段栏数据库类型</typeparam>
        /// <typeparam name="Table">数据库表数据库类型</typeparam>
        /// <param name="field">字段栏</param>
        /// <param name="gb">查询构建器</param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static bool ExIn<T, Table>(this T field, GetBuilder<Table> gb)
            where Table : IModel
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }

        /// <summary>
        /// 调用 SQL IN 操作符(Where)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="field">字段栏</param>
        /// <param name="gb"></param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static bool ExIn<T, T1>(this T field, IEnumerable<T1> gb)
            where T1 : struct
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }

        /// <summary>
        /// 调用 SQL IN 操作符(Where)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">字段栏</param>
        /// <param name="gb">字符串数组</param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static bool ExIn<T>(this T field, IEnumerable<string> gb)
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }

        /// <summary>
        /// 调用 SQL NOT IN 操作符(Where)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="Table"></typeparam>
        /// <param name="field">字段栏</param>
        /// <param name="gb"></param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static bool ExNotIn<T, Table>(this T field, GetBuilder<Table> gb)
            where Table : IModel
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }

        /// <summary>
        /// 调用 SQL NOT IN 操作符(Where)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="field">字段栏</param>
        /// <param name="gb"></param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static bool ExNotIn<T, T1>(this T field, IEnumerable<T1> gb)
            where T1 : struct
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }

        /// <summary>
        /// 调用 SQL NOT IN 操作符(Where)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">字段栏</param>
        /// <param name="gb"></param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static bool ExNotIn<T>(this T field, IEnumerable<string> gb)
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }

        /// <summary>
        /// 调用 SQL IS NULL 函数(Where,Having)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">字段栏</param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static bool ExIsNull<T>(this T field)
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }

        /// <summary>
        /// 调用  函数(Select)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">字段栏</param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static bool ExSql<T>(this T field)
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }

        /// <summary>
        /// 调用 SQL IS NOT NULL 函数(Where,Having)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">字段栏</param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static bool ExIsNotNull<T>(this T field)
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }

        ///// <summary>
        ///// 调用 SQL datediff 函数(Where)
        ///// </summary>
        ///// <param name="field">字段栏</param>
        ///// <param name="dt"></param>
        ///// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        ///// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        //public static bool ExIsCurrentDay(this DateTime field, DateTime dt)
        //{
        //    throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        //}

        ///// <summary>
        ///// 调用 SQL TIMESTAMPDIFF(Day,DateTime,DateTime) 函数(Where)
        ///// </summary>
        ///// <param name="field">字段栏</param>
        ///// <param name="dt">日期</param>
        ///// <param name="bflag">操作符(true为小于日期,false为大于日期)</param>
        ///// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        ///// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        //public static bool ExCompareDay(this DateTime field, DateTime dt, bool bflag)
        //{
        //    throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        //}

        ///// <summary>
        ///// 调用 SQL Year 函数(Where,Having)
        ///// </summary>
        ///// <param name="field">字段栏</param>
        ///// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        ///// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        //public static int ExYear(this DateTime field)
        //{
        //    throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        //}

        ///// <summary>
        ///// 调用 SQL Month 函数(Where,Having)
        ///// </summary>
        ///// <param name="field">字段栏</param>
        ///// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        ///// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        //public static int ExMonth(this DateTime field)
        //{
        //    throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        //}

        ///// <summary>
        ///// 调用 SQL Day 函数(Where,Having)
        ///// </summary>
        ///// <param name="field">字段栏</param>
        ///// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        ///// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        //public static int ExDay(this DateTime field)
        //{
        //    throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        //}

        ///// <summary>
        ///// 调用 SQL Date 函数(Where,Having)
        ///// </summary>
        ///// <param name="field">字段栏</param>
        ///// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        ///// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        //public static DateTime ExDate(this DateTime field)
        //{
        //    throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        //}

        /// <summary>
        /// 调用 SQL Sum 函数(Select,Having)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">字段栏</param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static T ExSum<T>(this T field) where T : struct
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }

        /// <summary>
        /// 调用 SQL Count 函数(Select,Having)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">字段栏</param>
        /// <param name="includeField">是否包含当前字段[false=COUNT(0) true=COUNT(field)]</param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static long ExCount<T>(this T field, bool includeField)
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }

        /// <summary>
        /// 调用 SQL Count 函数(Select,Having)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult">返回类型</typeparam>
        /// <param name="field">字段栏</param>
        /// <param name="includeField">是否包含当前字段[false=COUNT(0) true=COUNT(field)]</param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static TResult ExCount<T, TResult>(this T field, bool includeField) where TResult : struct
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }

        /// <summary>
        /// 调用 SQL AVG 函数(Select,Having)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">字段栏</param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static T ExAvg<T>(this T field)
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }

        /// <summary>
        /// 调用 SQL MAX 函数(Select,Having)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">字段栏</param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static T ExMax<T>(this T field)
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }

        /// <summary>
        /// 调用 SQL MIN 函数(Select,Having)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">字段栏</param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static T ExMin<T>(this T field)
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }

        /// <summary>
        /// 生成子查询(Select,Where)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gb">子查询</param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static T ExResolve<T>(this IGetBuilder gb)
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }

        /// <summary>
        /// 调用 SQL IFNULL 函数(Select,Where,Having)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">字段栏</param>
        /// <param name="defaultvalue">字段为NULL时的默认值</param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static T ExIfNull<T>(this T field, T defaultvalue)
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }

        /// <summary>
        /// lamuda表达式格式化(Where)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">字段栏</param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static T ExFormat<T>(this string field)
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }

        /// <summary>
        /// 生成序列号列(Select)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">字段栏</param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static int ExRowNo<T>(this T field)
            where T : IModel
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }

        /// <summary>
        /// 调用 SQL Case When Then Else End 函数(Select)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">字段栏</param>
        /// <param name="expression">例n=>n.Id.ExThen(1) == 2[When Id = 2 Then 1]</param>
        /// <param name="value">默认值</param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static dynamic ExCase<T>(this T field, dynamic value, params Expression<Func<T, dynamic>>[] expression)
            where T : IModel
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }

        /// <summary>
        /// 调用 SQL Case When Then Else End 函数(Select)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="field">字段栏</param>
        /// <param name="expression">例n=>n.Id.ExThen(1) == 2[When Id = 2 Then 1]</param>
        /// <param name="value">默认值</param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static dynamic ExCase<T, TEntity>(this T field, dynamic value, params Expression<Func<T, TEntity, dynamic>>[] expression)
            where T : IModel
            where TEntity : IModel
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }

        /// <summary>
        /// 调用 SQL Then 必须搭配ExCase使用(Select)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">字段栏</param>
        /// <param name="value">默认值</param>
        /// <returns>表示此SQL 函数的返回值类型。只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发 <see cref="NotSupportedException"/> 异常。</returns>
        /// <exception cref="NotSupportedException">只能在 SQL 构建器中调用，当直接调用此 SQL 函数时将引发此异常。</exception>
        public static bool ExThen<T>(this T field, dynamic value)
            where T : IModel
        {
            throw new NotSupportedException("不能直接调用此方法，只能在 SQL 构建器中调用。");
        }


        #endregion
    }
}