import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PublicRepositoryOverviewComponent } from './public-repository-overview.component';

describe('PublicRepositoryOverviewComponent', () => {
  let component: PublicRepositoryOverviewComponent;
  let fixture: ComponentFixture<PublicRepositoryOverviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PublicRepositoryOverviewComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PublicRepositoryOverviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
