import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.scss']
})
export class ForgotPasswordComponent implements OnInit {

  background: string;
  email: string; 

  constructor() { 
    const index = Math.floor(Math.random() * (5)) + 1;
    this.background = `background-${index}`; 
  }

  ngOnInit(): void {
  }

  onSubmit() {

  }

}
