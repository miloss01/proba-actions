import { Component } from '@angular/core';
import {
  AbstractControl, AsyncValidatorFn,
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule, ValidationErrors,
  ValidatorFn,
  Validators
} from "@angular/forms";
import {MatButtonModule} from "@angular/material/button";
import {MatFormFieldModule} from "@angular/material/form-field";
import {MatInputModule} from "@angular/material/input";
import {NgIf} from "@angular/common";
import {Observable, of} from "rxjs";
import {ChangePasswordDto, LoginCredentials} from "../../../models/models";
import {ActivatedRoute, Router} from "@angular/router";
import {UserService} from "../../../services/user.service";

@Component({
  selector: 'app-change-password-page',
  standalone: true,
    imports: [
        FormsModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        NgIf,
        ReactiveFormsModule
    ],
  templateUrl: './change-password-page.component.html',
  styleUrl: './change-password-page.component.css'
})
export class ChangePasswordPageComponent {
  errorMessage: string = "";
  changePasswordForm:FormGroup = new FormGroup({
    newPassword: new FormControl('', [Validators.required, this.passwordValidator()]),
    confirmPassword: new FormControl('', [Validators.required], [this.confirmPasswordValidator()])
  });

  constructor(private route: ActivatedRoute, private userService: UserService, private router: Router) {
  }

  submitForm(){
    this.errorMessage = '';
    if(this.changePasswordForm.valid){
      const token = this.route.snapshot.queryParamMap.get('token')!;
      const changePasswordDto:ChangePasswordDto = {
        newPassword: this.changePasswordForm.controls['newPassword'].value,
        token: token
      }
      this.userService.changePassword(changePasswordDto).subscribe({
        next: () => {
          this.router.navigate(["/home"]);
        },
        error: err => {
          this.errorMessage = err.error.message;
        }
      })
    }
  }

  private passwordValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if(control.value.length < 8){
        return {minLength:{value:control.value}};
      }
      if(control.value.length > 20){
        return {maxLength:{value:control.value}};
      }
      const whiteSpaceRegex = new RegExp("^(?!.* ).{6,20}$")
      if(!whiteSpaceRegex.test(control.value)){
        return {whitespace:{value:control.value}};
      }
      return null;
    };
  }

  private confirmPasswordValidator(): AsyncValidatorFn {
    return (control: AbstractControl): Observable<ValidationErrors | null> => {
      return of((() => {
        const passwordField = this.changePasswordForm.controls['newPassword'];
        if(passwordField?.valid){
          if(passwordField.value != control.value){
            return {passwordsNotMatching:{value:control.value}};
          }
        }
        return null;
      })());
    };
  }
}
