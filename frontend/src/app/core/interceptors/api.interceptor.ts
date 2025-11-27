import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../auth/auth.service';

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
    const authService = inject(AuthService);
    const token = authService.token();
    const baseUrl = authService.baseUrl();

    // Skip if request is already absolute
    if (req.url.startsWith('http')) {
        return next(req);
    }

    // Clone request with new URL and Auth header
    let headers = req.headers;
    if (token) {
        headers = headers.set('Authorization', token);
    }

    const apiReq = req.clone({
        url: `${baseUrl}${req.url}`,
        headers
    });

    return next(apiReq);
};
