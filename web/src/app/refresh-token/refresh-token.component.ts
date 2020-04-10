import { Component, OnInit, Output, EventEmitter } from "@angular/core";
import { Subscription, Observable, timer } from "rxjs";
import { first } from "rxjs/operators";
import { AuthenticationService } from '../services/auth.service';
import { AppConfig } from '../helpers/app-config';

@Component({
  selector: "app-refresh-token",
  template: ""
})
export class RefreshTokenComponent implements OnInit {
  private subscription: Subscription;

  @Output() TimerExpired: EventEmitter<any> = new EventEmitter<any>();

  remainingTime: number;
  minutes: number;
  seconds: number;
  everySecond: Observable<number> = timer(0, 60000);

  constructor(private authService: AuthenticationService) {}

  ngOnInit() {
    this.subscription = this.everySecond.subscribe(seconds => {
      var token = this.authService.currentToken();
      if (token) {
        var expirationSecs = Number(token.exp) * 1000;
        var currentSecs = Date.now();
        var thresholdSecs =
          currentSecs +
          Number(AppConfig.settings.app_renew_before_mins) * 60 * 1000;

        if (thresholdSecs >= expirationSecs) {
          this.authService
            .renewToken()
            .pipe(first())
            .subscribe(next => {
              this.authService.setTokens(next.token, next.renewalToken);
            });
        }
      }
    });
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
}
