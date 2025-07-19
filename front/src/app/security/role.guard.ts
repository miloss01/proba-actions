import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, CanActivateFn, Router, RouterStateSnapshot } from '@angular/router';
import { UserRole } from 'app/models/models';
import { AuthService } from 'app/services/auth.service';

@Injectable({
  providedIn: 'root',
})
export class RoleGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): boolean {
    const roles = route.data['roles'] as Array<string>;
    const userRole: UserRole | undefined = this.authService.userData.value?.userRole;

    if (roles.includes(userRole!)) {
      return true;
    } else {
      this.router.navigate(['/home']);
      return false;
    }
  }
}
