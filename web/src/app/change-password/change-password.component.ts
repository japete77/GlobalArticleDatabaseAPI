import { Component, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AuthenticationService } from '../services/auth.service';
import { MatDialog } from '@angular/material/dialog';
import { DialogComponent } from '../dialog/dialog.component';

@Component({
  selector: 'app-change-password',
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.scss']
})
export class ChangePasswordComponent implements OnInit {

  submitted: boolean;
  error: string;
  changePasswordForm: FormGroup;
  background: string;
  email: string;
  token: string;
  newpassword: string;
  renewpassword: string;
  loading = false

  constructor(
    private dialog: MatDialog,    
    private route: ActivatedRoute,
    private authenticationService: AuthenticationService) { 
    const index = Math.floor(Math.random() * (5)) + 1;
    this.background = `background-${index}`; 
  }

  ngOnInit(): void {
    this.submitted = false;
    this.email = this.route.snapshot.queryParams['email'];
    this.token = this.route.snapshot.queryParams['token'];
  }

  onSubmit() {
    this.submitted = true;
    this.error = '';

    // stop here if form is invalid
    if (!this.newpassword || !this.renewpassword || this.newpassword != this.renewpassword) {
      this.submitted = false;
      this.error = 'Password mismatch or empty';
      return;
    }

    if (!this.email || !this.token) {
      this.submitted = false;
      this.error = 'Invalid password change request';
      return;
    }

    this.loading = true    

    this.authenticationService.changePassword(this.email, this.token, this.newpassword)
    .toPromise()
    .then(data => {
      this.submitted = true;
    })
    .catch(data  => {
      this.submitted = false;

      const dialogRef = this.dialog.open(DialogComponent, {
        width: '300px',
        data: { title: 'Change password error', message: data.error.error.userMessage }
      });
  
      dialogRef.afterClosed().subscribe(result => {
        this.newpassword = '';
        this.renewpassword = '';
      });
    })
    .finally(() => {
      this.loading = false
    });
  }
}
