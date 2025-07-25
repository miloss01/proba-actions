export default interface RepositoryCreation {
    name:string
    owner:string
    description:string
    isPublic: boolean
}

export interface DescriptionRequest {
  repositoryId: string,
  newDescription: string
}

export interface VisibilityRequest {
  repositoryId: string,
  isPublic: boolean
}

export interface LoginCredentials {
  email: string,
  password: string
}
export interface UserData{
  userId: string,
  userEmail: string,
  userRole: UserRole
}
export interface RegisterUserDto{
  email: string,
  username: string,
  location?: string,
  password: string
}

export interface BaseUser{
  id: string,
  email: string,
  username: string,
  location?: string
}

export interface TeamsData {
  id: string,
  name: string,
  description: string
  members: Member[],
  organizationId: string
}

export interface Member {
  email: string
}

export interface TeamRepoPerm {
  permission: number,
  teamId: string,
  team: TeamsData,
  repositoryId: string,
  repository: RepositoryCreation
}

export enum UserRole{
  StandardUser = "StandardUser",
  Admin = "Admin",
  SuperAdmin = "SuperAdmin"
}

export interface ChangePasswordDto{
  newPassword: string,
  token: string
}

export interface DockerImageDTO {
  imageId: string;
  repositoryName: string;
  repositoryId: string;
  badge: string;
  starCount: number;
  description: string;
  tags: string[];
  lastPush: string;
  owner: string;
  createdAt: string;
  digest: string;
}

export interface PageDTO<T> {
  data: T[];
  totalNumberOfElements: number;
}

export interface DockerRepositoryDTO extends RepositoryCreation {
  id: string;
  starCount: number;
  badge: string;
  images: DockerImageDTO[];
  createdAt: string;
  lastPushed?: string
}

export interface MinifiedStandardUserDTO {
  id: string;
  username: string;
  badge: string;
}

export interface NewBadgeDTO {
  badge: string;
}
