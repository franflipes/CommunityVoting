export enum UserRole {
  GlobalAdmin = 0,
  CommunityAdmin = 1,
  CommunityMember = 2
}

export enum MeetingType {
  Ordinary = 0,
  Extraordinary = 1
}

export enum VotingState {
  Created = 0,
  Prepared = 1,
  Open = 2,
  Closed = 3,
  Expired = 4
}

export enum QuorumType {
  PercentageOfEligibleMembers = 1
}

export enum MajorityType {
  SimpleMajority = 1,
  MajorityOfVotesCast = 2,
  QualifiedMajority = 3
}

export enum AbstentionPolicy {
  Excluded = 1,
  IncludedInDenominator = 2,
  IncludedAsAgainst = 3
}

export interface User {
  id: string;
  name: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  role: UserRole;
}

export interface AuthResponse {
  token: string;
  user: User;
}

export interface VotingSettings {
  id: string;
  quorumEnabled: boolean;
  quorumType: QuorumType;
  quorumPercentage: number;
  requireQuorumForVoting: boolean;
  defaultMajorityType: MajorityType;
  defaultMajorityPercentage?: number;
  abstentionPolicy: AbstentionPolicy;
}

export interface Community {
  id: string;
  name: string;
  address: string;
  cif?: string;
  createdByUserId: string;
  createdByName?: string;
  membersCount?: number;
  memberCount?: number;
  votingSettings?: VotingSettings;
}

export interface CommunityMember {
  id: string;
  communityId: string;
  userId: string;
  memberRole: UserRole;
  userName: string;
  userLastName: string;
  userEmail: string;
  hasVotingRights: boolean;
  isActive: boolean;
  joinedAt: string;
}

export interface Document {
  id: string;
  proposalId: string;
  title: string;
  description?: string;
  fileName: string;
  contentType: string;
  fileSize: number;
  uploadedByUserId: string;
  uploadedByUserName: string;
  uploadedAt: string;
}

export interface ProposalOption {
  id: string;
  proposalId: string;
  label: string;
}

export interface Proposal {
  id: string;
  meetingId: string;
  agendaItemId: string;
  title: string;
  description?: string;
  order: number;
  majorityType?: MajorityType;
  majorityPercentage?: number;
  options: ProposalOption[];
  documents: Document[];
}

export interface AgendaItem {
  id: string;
  meetingId: string;
  title: string;
  description?: string;
  order: number;
  proposals: Proposal[];
}

export interface MeetingParticipant {
  id: string;
  meetingId: string;
  userId: string;
  userName: string;
  userLastName: string;
  userEmail: string;
  joinedAt: string;
  isPresent: boolean;
}

export interface QuorumStatus {
  meetingId: string;
  eligibleMembers: number;
  presentMembers: number;
  quorumRequired: number;
  quorumPercentage: number;
  quorumReached: boolean;
  requireQuorumForVoting: boolean;
}

export interface Meeting {
  id: string;
  communityId: string;
  communityName: string;
  title: string;
  type: MeetingType;
  location: string;
  scheduledAt: string;
  secondCallAt?: string;
  votingStart: string;
  votingEnd: string;
  isTransparent: boolean;
  votingSettings?: VotingSettings;
  agendaItems: AgendaItem[];
  proposals: Proposal[];
}

export interface VotingSession {
  id: string;
  proposalId: string;
  meetingId: string;
  communityId: string;
  agendaItemId?: string;
  agendaItemTitle?: string;
  title: string;
  description?: string;
  displayOrder: number;
  meetingName: string;
  state: VotingState;
  createdAt: string;
  openedAt?: string;
  closedAt?: string;
  eligibleMembers?: number;
  presentMembers?: number;
  quorumRequired?: number;
  quorumReached?: boolean;
  requireQuorumForVoting?: boolean;
  majorityType?: MajorityType;
  majorityPercentage?: number;
  options: ProposalOption[];
  ballots?: any[];
  liveStats?: {
    totalBallots: number;
    totalVotesCast: number;
    participationPercentage: number;
    votesByOption: Record<string, number>;
  };
}

export interface VotingResult {
  id: string;
  sessionId: string;
  originalProposalId: string;
  meetingId: string;
  communityId: string;
  agendaItemId?: string;
  agendaItemTitle?: string;
  title: string;
  closedAt: string;
  closedByUserName: string;
  eligibleMembers?: number;
  presentMembers?: number;
  quorumRequired?: number;
  quorumReached?: boolean;
  totalBallots: number;
  totalVotesCast: number;
  participationPercentage: number;
  majorityTypeUsed?: MajorityType;
  majorityPercentageUsed?: number;
  approved?: boolean;
  winningOptionId?: string;
  winningOptionLabel: string;
  votesByOption: Record<string, number>;
}

export interface CommunityInvitation {
  id: string;
  communityId: string;
  communityName: string;
  token: string;
  inviteUrl: string;
  createdByUserId: string;
  createdByUserName: string;
  createdAt: string;
  expiresAt?: string;
  maxUses: number;
  usesCount: number;
  isActive: boolean;
  isValid: boolean;
}

export interface VerifyInvitationResponse {
  isValid: boolean;
  token?: string;
  communityId?: string;
  communityName?: string;
  errorMessage?: string;
}

export interface MeetingVoterAccessDto {
  id: string;
  meetingId: string;
  userId: string;
  userName: string;
  userEmail: string;
  token?: string;
  code?: string;
  accessUrl: string;
  createdAt: string;
  expiresAt?: string;
  isRevoked: boolean;
  lastUsedAt?: string;
  failedAttempts: number;
}

export interface MeetingAccessAuthResponse {
  accessToken: string;
  expiresIn: number;
  redirectUrl: string;
  user: User;
}
