import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from "../env/environment";

@Injectable({
  providedIn: 'root'
})
export class OrganizationService {

  private apiUrl = `${environment.apiHost}organization`;

  constructor(private http: HttpClient) { }

  addOrganization(organization: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, organization);
  }

  getOrganizations(email: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/${email}`);
  }

  getMembersByOrganizationId(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}/members`);
  }

  addMemberToOrganization(organizationId: string, userId: string): Observable<any> {
    const body = {
      OrganizationId: organizationId,
      UserId: userId,
    };

    return this.http.post(`${this.apiUrl}/add-member`, body);
  }

  deleteOrganization(orgId: string): Observable<void> {
    const url = `${this.apiUrl}/delete/${orgId}`;
    return this.http.delete<void>(url);
  }

  updateOrganization(updateDto: { id: string, description: string, imageLocation: string }): Observable<any> {
    return this.http.put(`${this.apiUrl}/update`, updateDto);
  }

  // getOrganizationById(id: string): Observable<any> {
  //   return this.http.get<any>(`${this.apiUrl}/details/${id}`);
  // }
}
