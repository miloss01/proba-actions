import { Routes } from '@angular/router';
import { LandingPageComponent } from './modules/layout/landing-page/landing-page.component';
import { CreateRepositoryComponent } from './modules/repository/create-repository/create-repository.component';
import {LoginPageComponent} from "./modules/layout/login-page/login-page.component";
import {HomePageComponent} from "./modules/layout/home-page/home-page.component";
import { SingeRepositoryComponent } from './modules/repository/singe-repository/singe-repository.component';
import { AllRepositoriesComponent } from './modules/repository/all-repositories/all-repositories.component';
import {ChangePasswordPageComponent} from "./modules/layout/change-password-page/change-password-page.component";
import {ExplorePageComponent} from "./modules/layout/explore-page/explore-page.component";
import { PublicRepositoryOverviewComponent } from './modules/layout/public-repository-overview/public-repository-overview.component';
import {RegisterPageComponent} from "./modules/layout/register-page/register-page.component";
import { ListOrganizationsComponent } from './modules/organization/list-organizations/list-organizations.component';
import { DetailsComponent } from './modules/organization/details/details.component';
import { TeamsComponent } from './modules/layout/teams/all-teams/teams.component';
import { TeamDetailsComponent } from './modules/layout/teams/team-details/team-details.component';
import { PreventAuthGuard } from './security/prevent-auth.guard';
import { AuthGuard } from './security/auth.guard';
import { UserRole } from './models/models';
import { RoleGuard } from './security/role.guard';
import { UserBadgesComponent } from './modules/layout/user-badges/user-badges.component';
import { LogsComponent } from './modules/layout/logs/logs.component';

export const routes: Routes = [
  { path: 'all-user-repo', component: AllRepositoriesComponent, canActivate: [AuthGuard] },
  { path: 'single-repo/:id', component: SingeRepositoryComponent, canActivate: [AuthGuard] },
  { path: 'create-repo', component: CreateRepositoryComponent, canActivate: [AuthGuard] },
  { path: 'login', component: LoginPageComponent, canActivate: [PreventAuthGuard] },
  { path: 'home', component: HomePageComponent, canActivate: [AuthGuard] },
  { path: 'password/change', component: ChangePasswordPageComponent },
  { path: 'explore', component: ExplorePageComponent },
  { path: 'explore/repository/:id', component: PublicRepositoryOverviewComponent },
  { path: 'sign-up', component: RegisterPageComponent, canActivate: [PreventAuthGuard], data: { title: 'Sign Up', isAdmin: false } },
  { path: 'sign-up-admin', component: RegisterPageComponent, canActivate: [AuthGuard], data: { title: 'Sign Up Admin', isAdmin: true } },
  { path: 'organizations', component: ListOrganizationsComponent, canActivate: [AuthGuard] },
  { path: 'org-details/:id', component: DetailsComponent, canActivate: [AuthGuard] },
  { path: 'teams', component: TeamsComponent, canActivate: [AuthGuard] },
  { path: 'team-details/:id', component: TeamDetailsComponent, canActivate: [AuthGuard] },
  { path: 'badges', component: UserBadgesComponent, canActivate: [AuthGuard, RoleGuard], data: { roles: [UserRole.Admin, UserRole.SuperAdmin]} },
  { path: 'logs', component: LogsComponent, canActivate: [AuthGuard, RoleGuard], data: { roles: [UserRole.Admin, UserRole.SuperAdmin]} },
  { path: '**', component: LandingPageComponent }
];
