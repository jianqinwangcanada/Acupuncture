import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { UserProfileComponent } from './user-profile/user-profile.component';
import { UserActivityComponent } from './user-activity/user-activity.component';
import { UserSettingComponent } from './user-setting/user-setting.component';
const routes: Routes = [
  { path: '', component: UserProfileComponent },
  { path: 'myaccount', component: UserProfileComponent },
  { path: 'setting', component: UserSettingComponent },
  { path: 'activity', component: UserActivityComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UserRoutingModule { }
