import { Component } from '@angular/core';
import { MaterialModule } from 'app/infrastructure/material/material.module';
import {AuthService} from "../../../services/auth.service";
import {Router} from "@angular/router";
import {NgIf} from "@angular/common";
import {UserRole} from "../../../models/models";

@Component({
  selector: 'app-toolbar',
  standalone: true,
  imports: [MaterialModule, NgIf],
  templateUrl: './toolbar.component.html',
  styleUrl: './toolbar.component.css'
})
export class ToolbarComponent {
  userType?: UserRole;
  constructor(public authService: AuthService, private router: Router) {
    this.authService.userData.subscribe(value => {
      if(!value){
        this.userType = undefined;
      }else {
        this.userType = value.userRole;
      }
    });
  }

  logout(){
    this.authService.logout();
    this.router.navigate(["/"]);
  }
}
