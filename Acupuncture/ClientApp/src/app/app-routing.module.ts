import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AboutUsComponent } from './about-us/about-us.component';
import { ContactUsComponent } from './contact-us/contact-us.component';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { SendCodeComponent } from './send-code/send-code.component';
import { TermsComponent } from './terms/terms.component';
import { ValidateCodeComponent } from './validate-code/validate-code.component';

const routes: Routes = [
  { path: "home", component: HomeComponent },
  { path: "", component: HomeComponent, pathMatch:'full' },
  { path: "about-us", component: AboutUsComponent },
  { path: "contact-us", component: ContactUsComponent },
  { path: "forgot-password", component: ForgotPasswordComponent },
  { path: "login", component: LoginComponent },
  { path: "register", component: RegisterComponent },
  { path: "validate-code", component: ValidateCodeComponent },
  { path: "send-code", component: SendCodeComponent },
  { path: "terms", component: TermsComponent },
  { path: "myaccount", loadChildren:() => import(`./user/user.module`).then(m => m.UserModule)},
  { path: "**", component: HomeComponent }

];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
