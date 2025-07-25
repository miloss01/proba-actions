import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AllRepositoriesComponent } from './all-repositories.component';

describe('AllRepositoriesComponent', () => {
  let component: AllRepositoriesComponent;
  let fixture: ComponentFixture<AllRepositoriesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AllRepositoriesComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AllRepositoriesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
