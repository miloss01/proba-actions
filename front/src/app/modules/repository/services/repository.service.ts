import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'app/env/environment';
import RepositoryCreation, { DescriptionRequest, DockerRepositoryDTO, VisibilityRequest } from 'app/models/models';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class RepositoryService {

  constructor(private http: HttpClient) { }

  GetUsersRepositories(userId: string) : Observable<DockerRepositoryDTO[]> {
    const url = `${environment.apiHost}dockerRepositories/all/${userId}`;
    return this.http.get<DockerRepositoryDTO[]>(url);    
  }

  CreateRepository(requestRepo: RepositoryCreation) : Observable<DockerRepositoryDTO> {
    const url = `${environment.apiHost}dockerRepositories`;
    return this.http.post<DockerRepositoryDTO>(url, requestRepo);    
  }

  UpdateRepositoryDescription(descritionRequest: DescriptionRequest) : Observable<DockerRepositoryDTO> {
    const url = `${environment.apiHost}dockerRepositories/update-description`;
    return this.http.put<DockerRepositoryDTO>(url, descritionRequest);    
  }

  UpdateRepositoryVisibility(visibilityRequest: VisibilityRequest) : Observable<DockerRepositoryDTO> {
  const url = `${environment.apiHost}dockerRepositories/update-visibility`;
  return this.http.put<DockerRepositoryDTO>(url, visibilityRequest);    
}

  DeleteRepository(id: string) : Observable<DockerRepositoryDTO> {
  const url = `${environment.apiHost}dockerRepositories/delete/${id}`;
  return this.http.delete<DockerRepositoryDTO>(url);    
}

}

