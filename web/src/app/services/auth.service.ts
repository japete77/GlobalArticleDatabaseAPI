import { Injectable } from '@angular/core'
import { HttpClient } from '@angular/common/http'
import { map, first } from 'rxjs/operators'

import { CookieService } from '../services/cookie.service'
import { SecurityContext, AuthToken } from '../models/auth.models'
import { Router } from '@angular/router';
import { LoginResponse } from '../models/responses/login.response.model'
import { RenewResponse } from '../models/responses/renew.token.response';
import { AppConfig } from '../helpers/app-config'

@Injectable({ providedIn: 'root' })
export class AuthenticationService {
    private securityTokenKey = 'securityContext'
    private _securityContext: SecurityContext;
    private _decodedToken: AuthToken;

    constructor(private http: HttpClient, private cookieService: CookieService, private router: Router) {
    }

    /**
     * Returns the current security context
     */
    securityContext(): SecurityContext {
        if (!this._securityContext) {
            this._securityContext = JSON.parse(this.cookieService.getCookie(this.securityTokenKey))
        }
        return this._securityContext
    }

    currentToken(): AuthToken {
        if (!this._decodedToken) {
            var securityContext = this.securityContext();
            if (securityContext) {
                this._decodedToken = this.parseJwt(securityContext.token)
            }
        }
        return this._decodedToken
    }

    /**
     * Performs the auth
     * @param email email of user
     * @param password password of user
     */
    login(user: string, password: string) {
        
        console.log(`${AppConfig.settings.api_base_url}`)
        console.log(`${AppConfig.settings.api_version}`)
        return this.http.post<LoginResponse>(`${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/auth/login`, { user, password })
            .pipe(map(response => {
                // login successful if there's a jwt token in the response
                if (response && response.token) {
                    this.setTokens(response.token, response.renewToken)
                }
                return response;
            }));
    }

    /**
     * Performs the forgot password action
     * @param email email of user
     * @param tenant tenant of user
     */
    forgotPassword(email: string) {
        return this.http.post<any>(`${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/auth/forgot-password`, { email })
    }

    /**
     * Performs the forgot password action
     * @param email email of user
     * @param tenant tenant of user
     */
    changePassword(email: string, resetPasswordToken: string, newPassword: string) {
        return this.http.post<any>(`${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/auth/reset-password`, { email, newPassword, resetPasswordToken })            
    }

    renewToken() {
        return this.http.post<RenewResponse>(`${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/auth/renew-token`, { renewToken: this.securityContext().renewToken })       
    }

    setTokens(userToken: string, renewToken: string) {
        this._securityContext = {
            token: userToken,
            renewToken: renewToken
        }
        
        this._decodedToken = this.parseJwt(userToken)

        // store user details and jwt in a session cookie
        this.cookieService.setCookie(this.securityTokenKey, JSON.stringify(this._securityContext), null)
    }

    /**
     * Logout the user
     * @param redirect flag to redirect to login page after logout
     */
    logout(redirect: boolean = true) {
        if (this.securityContext()) {
            // call to backend to remove renew token
            this.http.post<any>(`${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/auth/logout`, { })
                .pipe(first())
                .subscribe(next => { }, error => { },
                    () => {
                        // remove security cookie to log user out                        
                        this.cleanup();

                        if (redirect) {
                            this.redirectLogin();
                        }
                    }
                )
        }
    }

    cleanup() {
        this.cookieService.deleteCookie(this.securityTokenKey)
        this._securityContext = null
        this._decodedToken = null;
    }

    redirectLogin() {
        const loginUrl = `/login`
        window.location.assign(loginUrl);
    }

    /**
     * Decode authentucation token from base64 to json object
     * @param token Authentication token
     */
    private parseJwt (token) {
        var base64Url = token.split('.')[1]
        var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
        var jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
        }).join(''))
    
        return JSON.parse(jsonPayload);
    };
}

