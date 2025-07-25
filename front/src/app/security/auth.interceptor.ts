import { HttpInterceptorFn } from '@angular/common/http';
import {AuthService} from "../services/auth.service";
import {inject} from "@angular/core";
import {Router} from "@angular/router";
import {JwtHelperService} from "@auth0/angular-jwt";
import {catchError, throwError} from "rxjs";

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Skip adding JWT if 'skip' header exists
  if (req.headers.has('skip')) {
    const cloned = req.clone({ headers: req.headers.delete('skip') });
    return next(cloned);
  }

  const accessToken = authService.getAccessToken();

  if (accessToken) {
    const helper = new JwtHelperService();
    const tokenType = helper.decodeToken(accessToken).type;
    const cloned = req.clone({
      headers: req.headers.set('authorization', `${tokenType} ${accessToken}`),
    });

    return next(cloned).pipe(
      catchError((error) => {
        if (
          error.status === 401 &&
          !cloned.url.includes('auth/login')
        ) {
          router.navigate(['/login']);
        }
        return throwError(() => error);
      })
    );
  }

  return next(req);
};
