import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from "../env/environment";

@Injectable({
  providedIn: 'root'
})
export class LogsService {

  private apiUrl = `${environment.apiHost}log`;

  constructor(private http: HttpClient) { }

  searchLogs(dto: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/search`, dto);
  }
}
