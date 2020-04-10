import { Component } from '@angular/core';
import { LoginRequest } from '../models/requests/login.request.model';
import { TdLoadingService } from '@covalent/core/loading';
import { AuthenticationService } from '../services/auth.service';
import { MatDialog } from '@angular/material/dialog';
import { DialogComponent } from '../dialog/dialog.component';
import { Router } from '@angular/router';

@Component({
  selector: 'login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {

  background: string;
  credentials: LoginRequest = { user: '', password: '' };

  constructor(private loadingService: TdLoadingService, 
    private authService: AuthenticationService, 
    private dialog: MatDialog,
    private router: Router) { 
    const index = Math.floor(Math.random() * (5)) + 1;
    this.background = `background-${index}`; 
  }

  onSubmit() {
    this.loadingService.register("loading-login");
    this.authService.login(this.credentials.user, this.credentials.password)
      .toPromise()
      .then(data => {
        this.router.navigateByUrl('/');
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
        this.loadingService.resolve("loading-login");
      });
  }
}
