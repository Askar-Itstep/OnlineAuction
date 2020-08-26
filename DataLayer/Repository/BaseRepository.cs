using OnlineAuction.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Repository
{
    public class BaseRepository<T>: IRepository<T> where T: class
    {
        private Model1 db;
        private DbSet<T> dbSet;
        public BaseRepository()
        {
            db = new Model1();
            dbSet = db.Set<T>();
        }
        public BaseRepository(Model1 db)
        {
            this.db = db;
            dbSet = db.Set<T>();
        }
        public void Create(T item)
        {
            dbSet.Add(item);
        }

        public void Delete(int item)
        {
            var entity = dbSet.Find(item);
            dbSet.Remove(entity);
        }

        public IQueryable<T> GetAll()
        {
            return dbSet;
        }

        public T GetById(int? id)
        {
            return dbSet.Find(id);
        }

        public IQueryable<T> Include(params string[] navigationProperty)
        {
            var query = GetAll();
            foreach (var item in navigationProperty)
                query.Include(item);
            return query;
        }

        public void Save()
        {
            db.SaveChanges();
        }

        public void Update(T item)
        {
            dbSet.Attach(item);
            db.Entry(item).State = EntityState.Modified;
        }

        public IQueryable<T> GetAllNoTracking()
        {
            return dbSet.AsNoTracking();
        }
    }
}
