import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'app/env/environment';
import { DockerRepositoryDTO, PageDTO } from 'app/models/models';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class RepositoryService {

  constructor(private http: HttpClient) { }
  
  getDockerRepositoryById(id: string) : Observable<DockerRepositoryDTO> {
    return this.http.get<DockerRepositoryDTO>(`${environment.apiHost}dockerRepositories/${id}`);
  }

  getStarDockerRepositoriesForUser(userId: string) : Observable<DockerRepositoryDTO[]> {
    return this.http.get<DockerRepositoryDTO[]>(`${environment.apiHost}dockerRepositories/star/${userId}`);
  }

  getPrivateDockerRepositoriesForUser(userId: string) : Observable<DockerRepositoryDTO[]> {
    return this.http.get<DockerRepositoryDTO[]>(`${environment.apiHost}dockerRepositories/private/${userId}`);
  }

  starRepository(userId: string, repositoryId: string) : Observable<void> {
    return this.http.patch<void>(`${environment.apiHost}dockerRepositories/star/${userId}/${repositoryId}`, {});
  }

  removeStarRepository(userId: string, repositoryId: string) : Observable<void> {
    return this.http.patch<void>(`${environment.apiHost}dockerRepositories/star/remove/${userId}/${repositoryId}`, {});
  }

  getNotAllowedToStarRepositoriesForUser(userId: string) : Observable<string[]> {
    return this.http.get<string[]>(`${environment.apiHost}dockerRepositories/star/notallowed/${userId}`);
  }

  getDockerRepositories(page: number, pageSize: number, searchTerm: string, badges: string): Observable<PageDTO<DockerRepositoryDTO>>{
      return this.http.get<PageDTO<DockerRepositoryDTO>>(`${environment.apiHost}dockerRepositories?page=${page}&pageSize=${pageSize}&searchTerm=${searchTerm}&badges=${badges}`);
    }
}
