import { Directive, Input, ElementRef, OnInit } from '@angular/core';
import { AuthenticationService } from '../services/auth.service';

@Directive({
  selector: '[secure]'
})
export class SecureDirective implements OnInit {

  @Input() roles: string[];

  private elRef:ElementRef;

  constructor(el: ElementRef, private authService: AuthenticationService) { 
      this.elRef = el;
  }

  ngOnInit() {
    let roles = this.authService.currentToken().role;
    if (!Array.isArray(roles)) {
      roles = [ roles ];
    }

    let intersect = roles.filter(x => {
      if (this.roles && Array.isArray(this.roles) && this.roles.indexOf(x) != -1)
        return true;
      else
        return false;
    });

    if (!intersect || intersect.length == 0) {
      this.elRef.nativeElement.style.display = 'none';
    }
  }

}
