import { Component } from '@angular/core';
import { LoginRequest } from '../models/requests/login.request.model';
import { AuthenticationService } from '../services/auth.service';
import { MatDialog } from '@angular/material/dialog';
import { DialogComponent } from '../dialog/dialog.component';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {

  subscription: any;
  returnUrl: string;
  background: string;
  credentials: LoginRequest = { user: '', password: '' };
  loading = false

  constructor(
    private authService: AuthenticationService, 
    private dialog: MatDialog,
    private route: ActivatedRoute,
    private router: Router) { 
    const index = Math.floor(Math.random() * (5)) + 1;
    this.background = `background-${index}`;

    this.subscription = this.route.queryParams.subscribe(params => {
      this.returnUrl = params['returnUrl']
      console.log(this.returnUrl)
    });
  }

  onSubmit() {
    this.loading = true
    this.authService.login(this.credentials.user, this.credentials.password)
      .toPromise()
      .then(data => {
        if (this.returnUrl) {
          this.router.navigateByUrl(this.returnUrl);
        } else {
          this.router.navigateByUrl('/');
        }        
      })
      .catch(data => {
        const dialogRef = this.dialog.open(DialogComponent, {
          width: '300px',
          data: { title: 'Access error', message: data.error.error.userMessage }
        });
    
        dialogRef.afterClosed().subscribe(result => {
          this.credentials.user = '';
          this.credentials.password = '';
        });
      })
      .finally(() => {
        this.loading = false
      });
  }
}
