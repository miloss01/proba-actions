import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChangeVisibilityPopupComponent } from './change-visibility-popup.component';

describe('ChangeVisibilityPopupComponent', () => {
  let component: ChangeVisibilityPopupComponent;
  let fixture: ComponentFixture<ChangeVisibilityPopupComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChangeVisibilityPopupComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ChangeVisibilityPopupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
