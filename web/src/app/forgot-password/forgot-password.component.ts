import { Component, OnInit } from '@angular/core';
import { AuthenticationService } from '../services/auth.service';
import { MatDialog } from '@angular/material/dialog';
import { DialogComponent } from '../dialog/dialog.component';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.scss']
})
export class ForgotPasswordComponent implements OnInit {

  background: string;
  email: string; 
  emailSent: boolean;
  loading = false

  constructor(
    private authService: AuthenticationService, 
    private dialog: MatDialog) { 
    const index = Math.floor(Math.random() * (5)) + 1;
    this.background = `background-${index}`; 
    this.emailSent = false;
  }

  ngOnInit(): void {
  }

  onSubmit() {
    this.loading = true
    this.authService.forgotPassword(this.email)
      .toPromise()
      .then(data => {
        this.emailSent = true;
      })
      .catch(data => {
        const dialogRef = this.dialog.open(DialogComponent, {
          width: '300px',
          data: { title: 'Forgot password error', message: data.error.error.userMessage }
        });
    
        dialogRef.afterClosed().subscribe(result => {
          this.email = '';
        });
      })
      .finally(() => {
        this.loading = false
      });  
  }
}
