import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { UserRoutingModule } from './user-routing.module';
import { UserProfileComponent } from './user-profile/user-profile.component';
import { UserActivityComponent } from './user-activity/user-activity.component';
import { UserSettingComponent } from './user-setting/user-setting.component';


@NgModule({
  declarations: [UserProfileComponent, UserActivityComponent, UserSettingComponent],
  imports: [
    CommonModule,
    UserRoutingModule
  ]
})
export class UserModule { }
