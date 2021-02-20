using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Acupuncture.Model;

namespace Acupuncture.CommonFunction.ActivityFunction
{
    public interface IActivitySvc
    {

        public  Task AddUserActivity(Activity activityModel);


        public  Task<List<Activity>> GetUserActivities(string useId);
        


        }
}
