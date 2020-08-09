import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable } from 'rxjs';

import { AuthenticationService } from '../services/auth.service';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
    constructor(private authenticationService: AuthenticationService) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

        var currentToken = this.authenticationService.currentToken()
        const securityContext = this.authenticationService.securityContext();

        if (currentToken && this.tokenHasExpired(currentToken)) {
            this.authenticationService.cleanup();
            this.authenticationService.redirectLogin();
            return next.handle(request);
        }

        // add authorization header with jwt token if available
        if (securityContext && securityContext.token) {
            request = request.clone({
                setHeaders: {
                    Authorization: `Bearer ${securityContext.token}`
                }
            });
        }

        return next.handle(request);
    }

    private tokenHasExpired(token) : boolean {
        if (token) {
            var expirationSecs = Number(token.exp) * 1000;
            var currentSecs = Date.now();

            return (currentSecs >= expirationSecs);
        } else {
            return true;
        }
    }
}
