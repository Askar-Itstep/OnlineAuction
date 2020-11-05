using OnlineAuction.Entities;
using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;

namespace DataLayer.Repository
{
    public class BaseRepository<T> : IRepository<T> where T : class
    {
        private Model1 db;
        private DbSet<T> dbSet;
        //private Model2 db2;
        public BaseRepository()
        {
            db = new Model1();  //Azure
            dbSet = db.Set<T>();
        }
        public BaseRepository(Model1 db)    //AWS
        {
            this.db = db;
            dbSet = db.Set<T>();
        }
        public void Create(T item)
        {
            dbSet.Add(item);
        }

        public void Delete(int? item)
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
            if (id == null)
            {
                return null;
            }

            return dbSet.Find(id);
        }
        public T GetLast()
        {
            return dbSet.ToList().LastOrDefault();
        }
        public IQueryable<T> Include(params string[] navigationProperty)
        {
            var query = GetAll();
            foreach (var item in navigationProperty)
            {
                query.Include(item);
            }

            return query;
        }

        public void Save()
        {
            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                {
                    System.Diagnostics.Debug.WriteLine("Object: " + validationError.Entry.Entity.ToString());

                    foreach (DbValidationError err in validationError.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine(err.ErrorMessage + "");
                    }
                }
            }
        }

        public void Update(T item)
        {
            try
            {
                dbSet.Attach(item);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: ", e.Message);
            }

            db.Entry(item).State = EntityState.Modified;
        }

        public IQueryable<T> GetAllNoTracking()
        {
            return dbSet.AsNoTracking();
        }

    }
}
