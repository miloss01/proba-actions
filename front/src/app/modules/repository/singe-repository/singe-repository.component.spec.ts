import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SingeRepositoryComponent } from './singe-repository.component';

describe('SingeRepositoryComponent', () => {
  let component: SingeRepositoryComponent;
  let fixture: ComponentFixture<SingeRepositoryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SingeRepositoryComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SingeRepositoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
