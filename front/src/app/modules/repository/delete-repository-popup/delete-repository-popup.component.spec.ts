import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeleteRepositoryPopupComponent } from './delete-repository-popup.component';

describe('DeleteRepositoryPopupComponent', () => {
  let component: DeleteRepositoryPopupComponent;
  let fixture: ComponentFixture<DeleteRepositoryPopupComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DeleteRepositoryPopupComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DeleteRepositoryPopupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
