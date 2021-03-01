using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acupuncture.Data;
using Acupuncture.Model;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Acupuncture.CommonFunction.ActivityFunction
{
    public class ActivitySvc:IActivitySvc
    {
        private readonly ApplicationDbContext _db;
        public ActivitySvc(ApplicationDbContext db)
        {
            _db = db;
        }
        public  async Task AddUserActivity(Activity activityModel)
        {
            using var dbTransaction = await _db.Database.BeginTransactionAsync();
            try
            {
                    await _db.activities.AddAsync(activityModel);
                    await _db.SaveChangesAsync();
                    await dbTransaction.CommitAsync();
                    
                
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                Log.Error("Error while add activitity {Error} {StackTrace} {InnerException} {Source}",
                   ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
        }
        public async Task<List<Activity >> GetUserActivities(string useId)
        {
            List<Activity> activities = new List<Activity>();
            try
            {
                using var dbTransaction = await _db.Database.BeginTransactionAsync();
                activities = await _db.activities.Where(o => o.UserId == useId).OrderByDescending(x => x.Date).Take(10).ToListAsync();


            }
            catch (Exception ex)
            {
                Log.Error("Error while Fetch  activitity from database {Error} {StackTrace} {InnerException} {Source}",
                  ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            return activities;
        }
    }
}
