using BeerBubbleUtility;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace BeerBubbleWeb
{
    public class BaseTable<T>:ITable<T> where T : BaseEntity
    {
        private string _fileOrServerOrConnection { get; set; }
        private static MappingSource mappingSource = new AttributeMappingSource();

        public BaseTable(string fileOrServerOrConnection)
        {
            _fileOrServerOrConnection = fileOrServerOrConnection;
        }

        /// <summary>
        /// 获取所有对象列表
        /// </summary>
        /// <returns></returns>
        public virtual List<T> GetAllList()
        {
            using (var db = new DataContext(_fileOrServerOrConnection, mappingSource))
            {
                var table = db.GetTable<T>();
                return table.ToList();

            }
        }

        public virtual int TotalCount()
        {
            using (var db = new DataContext(_fileOrServerOrConnection, mappingSource))
            {
                var table = db.GetTable<T>();
                return table.Count();

            }
        }

        /// <summary>
        /// 根据条件和加载项获取实体对象
        /// </summary>
        /// <param name="predicate">过滤条件</param>
        /// <param name="options">加载项</param>
        /// <returns></returns>
        public virtual T GetEntity(Expression<Func<T, bool>> predicate, DataLoadOptions options = null)
        {
            return GetEntity(predicate, false, options);
        }

        /// <summary>
        /// 根据条件和加载项获取实体对象，根据参数决定是否引发结果null异常
        /// </summary>
        /// <param name="predicate">过滤条件</param>
        /// <param name="throwExpOnResultIsNull">当结果为空时是否引发异常</param>
        /// <param name="options">加载项</param>
        /// <returns></returns>
        public virtual T GetEntity(Expression<Func<T, bool>> predicate, bool throwExpOnResultIsNull, DataLoadOptions options = null)
        {
            if (predicate == null) throw new ArgumentNullException("predicate", "过滤条件不能为空");
            using (DataContext db = new DataContext(_fileOrServerOrConnection, mappingSource))
            {
                db.ObjectTrackingEnabled = false;
                db.LoadOptions = options;
                var entity = db.GetTable<T>().FirstOrDefault(predicate);
                if (throwExpOnResultIsNull && entity == null) throw new Exception("指定条件的对象不存在！");
                return entity;
            }
        }

        /// <summary>
        /// 添加单个实体对象
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual T InsertEntity(T entity)
        {
            using (DataContext db = new DataContext(_fileOrServerOrConnection, mappingSource))
            {
                Table<T> table = db.GetTable<T>();
                table.InsertOnSubmit(entity);
                db.SubmitChanges();
                return entity;
            }
        }

        /// <summary>
        /// 批量添加实体对象列表
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual List<T> BatchInsertEntity(List<T> entities)
        {
            if (entities == null || entities.Count() == 0) throw new ArgumentException("要添加的集合不能为空！");
            using (DataContext db = new DataContext(_fileOrServerOrConnection, mappingSource))
            {
                db.GetTable<T>().InsertAllOnSubmit(entities);
                db.SubmitChanges();
                return entities;
            }
        }

        /// <summary>
        /// 根据委托更新指定条件的实体对象
        /// </summary>
        /// <param name="predicate">条件</param>
        /// <param name="action">要进行更新的操作</param>
        /// <returns>更新成功与否</returns>
        public virtual bool UpdateEntity(Expression<Func<T, bool>> predicate, Action<T> action)
        {
            if (predicate == null) throw new ArgumentNullException("predicate", "过滤条件不能为空");
            if (action == null) throw new ArgumentNullException("action", "更新操作委托方法不能为空");
            using (DataContext db = new DataContext(_fileOrServerOrConnection, mappingSource))
            {
                var entities = db.GetTable<T>().Where(predicate);
                foreach (var item in entities)
                {
                    action(item);
                }

                try
                {
                    db.SubmitChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                    //throw;
                }

            }
        }

        /// <summary>
        /// 根据条件和加载选项获取对象列表
        /// </summary>
        /// <param name="predicate">过滤条件</param>
        /// <param name="options">加载项</param>
        /// <returns></returns>
        public virtual List<T> GetEntityList(Expression<Func<T, bool>> predicate, DataLoadOptions options = null)
        {
            if (predicate == null) throw new ArgumentNullException("predicate", "过滤条件不能为空");
            using (DataContext db = new DataContext(_fileOrServerOrConnection, mappingSource))
            {
                db.ObjectTrackingEnabled = false;
                db.LoadOptions = options;
                return db.GetTable<T>().Where(predicate).ToList();
            }
        }


        /// <summary>
        /// 根据条件删除实体对象
        /// </summary>
        /// <param name="predicate">条件</param>
        /// <returns>删除成功与否</returns>
        public virtual bool DeleteEntity(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) throw new ArgumentNullException("predicate", "过滤条件不能为空");
            using (var db = new DataContext(_fileOrServerOrConnection, mappingSource))
            {
                Table<T> table = db.GetTable<T>();
                foreach (var item in table.Where(predicate))
                {
                    table.DeleteOnSubmit(item);
                }
                try
                {
                    db.SubmitChanges();
                    return true;
                }
                catch (Exception e)
                {
                    LogHelper.Current.Error(e);
                    return false;
                }
            }
        }

        /// <summary>
        /// 根据条件获取分页数据
        /// </summary>
        /// <param name="predicate">条件</param>
        /// <param name="predicate">要加载的数据选项</param>
        /// <param name="index">页码</param>
        /// <param name="size">页显示数目</param>
        /// <param name="count">总页数</param>
        /// <returns>分页数据</returns>
        public virtual List<T> GetPagedList(Expression<Func<T, bool>> predicate, out int count, int pageIndex = 1, int pageSize = 20)
        {
            if (pageIndex <= 0) throw new ArgumentException("起始页索引必须大于0", "index");
            if (pageSize <= 0) throw new ArgumentException("页显示数据大小必须大于0", "index");
            using (DataContext db = new DataContext(_fileOrServerOrConnection, mappingSource))
            {
                db.ObjectTrackingEnabled = false;
                var query = db.GetTable<T>().AsQueryable();
                if (predicate != null)
                {
                    query = query.Where(predicate);
                }
                return QueryToList(query, pageIndex, pageSize, out count);
            }
        }

        /// <summary>
        /// 根据条件和加载项获取排序对象列表
        /// </summary>
        /// <typeparam name="TKey">要排序对象的类型</typeparam>
        /// <param name="predicate">过滤条件</param>
        /// <param name="keySelector">要排序的对象</param>
        /// <param name="index">页序列</param>
        /// <param name="size">页数据显示大小</param>
        /// <param name="count">返回总页数</param>
        /// <param name="options">加载对象</param>
        /// <param name="orderByAsc">排列顺序</param>
        /// <returns></returns>
        public virtual List<T> GetSortedList<TKey>(Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> keySelector, bool orderByAsc, out int count, int pageIndex=1, int pageSize=20)
        {
            if (pageIndex <= 0) throw new ArgumentException("起始页索引必须大于0", "index");
            if (pageSize <= 0) throw new ArgumentException("页显示数据大小必须大于0", "index");
            using (DataContext db = new DataContext(_fileOrServerOrConnection, mappingSource))
            {
                db.ObjectTrackingEnabled = false;
                var query = db.GetTable<T>().AsQueryable();
                if (predicate != null)
                {
                    query = query.Where(predicate);
                }
                if (orderByAsc)
                {
                    query = query.OrderBy<T, TKey>(keySelector);
                }
                else
                {
                    query = query.OrderByDescending<T, TKey>(keySelector);
                }
                count = query.Count();
                if (count == 0) return new List<T>();
                count = (int)Math.Ceiling(count * 1.0 / pageSize);
                if (pageIndex > count) return new List<T>();
                pageIndex = pageIndex > 0 ? pageIndex : 1;
                return query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
        }

        /// <summary>
        /// 判断是否包含指定条件的数据
        /// </summary>
        /// <param name="predicate">条件</param>
        /// <returns></returns>
        public virtual bool Exsit(Expression<Func<T, bool>> predicate, bool throwExpOnResultIsNull = false)
        {
            if (predicate == null) throw new ArgumentNullException("predicate", "过滤条件不能为空");
            using (DataContext db = new DataContext(_fileOrServerOrConnection, mappingSource))
            {
                var result = db.GetTable<T>().Any(predicate);
                if (throwExpOnResultIsNull && !result) throw new Exception("指定条件的对象不存在！");
                return result;
            }
        }


        /// <summary>
        /// 将查询转换为集合对象
        /// </summary>
        /// <returns></returns>
        protected List<T> QueryToList(IQueryable<T> query, int pageIndex, int pageSize, out int count)
        {
            count = query.Count();
            if (count == 0) return new List<T>();
            count = (int)Math.Ceiling(count * 1.0 / pageSize);
            if (pageIndex > count) return new List<T>();
            pageIndex = pageIndex > 0 ? pageIndex : 1;
            return query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }
    }
}
