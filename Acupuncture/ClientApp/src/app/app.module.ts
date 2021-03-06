import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AboutUsComponent } from './about-us/about-us.component';
import { ContactUsComponent } from './contact-us/contact-us.component';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './login/login.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { RegisterComponent } from './register/register.component';
import { SendCodeComponent } from './send-code/send-code.component';
import { TermsComponent } from './terms/terms.component';
import { UserComponent } from './user/user.component';
import { ValidateCodeComponent } from './validate-code/validate-code.component';

@NgModule({
  declarations: [
    AppComponent,
    AboutUsComponent,
    ContactUsComponent,
    ForgotPasswordComponent,
    HomeComponent,
    LoginComponent,
    NavMenuComponent,
    RegisterComponent,
    SendCodeComponent,
    TermsComponent,
    UserComponent,
    ValidateCodeComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
