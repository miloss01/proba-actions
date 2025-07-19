import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'app/env/environment';
import { Member, TeamRepoPerm, TeamsData } from 'app/models/models';
import { Observable, ObservedValueOf } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TeamService {

  constructor(private http: HttpClient) { }

  getTeams(id: string): Observable<TeamsData[]>{
    return this.http.get<TeamsData[]>(`${environment.apiHost}team/org/${id}`);
  }

  getTeam(id: string): Observable<TeamsData> {
    return this.http.get<TeamsData>(`${environment.apiHost}team/${id}`);
  }

  deleteTeam(id: string): Observable<TeamsData> {
    return this.http.delete<TeamsData>(`${environment.apiHost}team/${id}`);
  }

  update(team: TeamsData): Observable<TeamsData> {
    return this.http.put<TeamsData>(`${environment.apiHost}team/${team.id}`, {"Name": team.name, "Description": team.description});
  }

  createTeam(team: TeamsData): Observable<TeamsData> {
    return this.http.post<TeamsData>(`${environment.apiHost}team`, team);
  }

  getRepositories(id: string): Observable<TeamRepoPerm[]> {
    return this.http.get<TeamRepoPerm[]>(`${environment.apiHost}team/repositories/${id}`);
  }
}
