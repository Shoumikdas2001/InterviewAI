// Auth Types
export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: 'User' | 'Admin';
  isActive: boolean;
  totalInterviews: number;
  averageScore: number;
  createdAt: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

// Resume Types
export interface AtsAnalysis {
  atsScore: number;
  formattingScore: number;
  keywordScore: number;
  experienceScore: number;
  strengths: string[];
  weaknesses: string[];
  missingKeywords: string[];
  recommendations: string[];
}

export interface ParsedResumeContent {
  fullName: string;
  email: string;
  phone: string;
  location: string;
  summary: string;
  skills: string[];
  totalYearsExperience: number;
}

export interface Resume {
  id: string;
  userId: string;
  fileName: string;
  fileSizeBytes: number;
  uploadedAt: string;
  analyzedAt?: string;
  parsedContent?: ParsedResumeContent;
  atsAnalysis?: AtsAnalysis;
}

export interface ResumeUploadResponse {
  id: string;
  fileName: string;
  fileSizeBytes: number;
  uploadedAt: string;
  message: string;
}

// Interview Types
export type InterviewType = 'Technical' | 'Behavioral' | 'SystemDesign' | 'Mixed';
export type DifficultyLevel = 'Easy' | 'Medium' | 'Hard';
export type InterviewStatus = 'Created' | 'InProgress' | 'Completed' | 'Abandoned';

export interface CreateInterviewRequest {
  jobRole: string;
  experienceLevel: string;
  skills: string[];
  interviewType: InterviewType;
  difficulty: DifficultyLevel;
  resumeId?: string;
}

export interface InterviewSession {
  id: string;
  title: string;
  jobRole: string;
  experienceLevel: string;
  skills: string[];
  interviewType: string;
  difficulty: string;
  status: InterviewStatus;
  overallScore?: number;
  technicalScore?: number;
  communicationScore?: number;
  confidenceScore?: number;
  problemSolvingScore?: number;
  durationMinutes?: number;
  createdAt: string;
  startedAt?: string;
  completedAt?: string;
  skillGapAnalysis?: SkillGapAnalysis;
}

export interface Question {
  id: string;
  sessionId: string;
  questionText: string;
  category: string;
  skillTag: string;
  orderIndex: number;
  isFollowUp: boolean;
  parentQuestionId?: string;
  expectedKeyPoints: string[];
}

export interface AnswerEvaluation {
  overallScore: number;
  technicalScore: number;
  communicationScore: number;
  confidenceScore: number;
  clarityScore: number;
  problemSolvingScore: number;
  completenessScore: number;
  feedback: string[];
  improvementAreas: string[];
  suggestedFollowUps: string[];
}

export interface Answer {
  id: string;
  questionId: string;
  sessionId: string;
  answerText: string;
  isVoiceAnswer: boolean;
  durationSeconds?: number;
  evaluation?: AnswerEvaluation;
  submittedAt: string;
}

export interface SubmitAnswerRequest {
  questionId: string;
  sessionId: string;
  answerText: string;
  isVoiceAnswer?: boolean;
  transcript?: string;
  durationSeconds?: number;
}

export interface SkillGapAnalysis {
  strengths: string[];
  weaknesses: string[];
  missingTopics: string[];
  recommendations: string[];
  skillScores: Record<string, number>;
}

// Study Plan Types
export interface StudyTopic {
  name: string;
  description: string;
  priority: 'High' | 'Medium' | 'Low';
}

export interface StudyWeek {
  weekNumber: number;
  theme: string;
  topics: StudyTopic[];
  resources: string[];
  exercises: string[];
  estimatedHours: number;
}

export interface StudyPlan {
  id: string;
  title: string;
  jobRole: string;
  targetSkills: string[];
  estimatedHours: number;
  weeks: StudyWeek[];
  createdAt: string;
}

// Dashboard Types
export interface ScoreTrend {
  date: string;
  score: number;
  jobRole: string;
}

export interface SkillPerformance {
  skill: string;
  score: number;
  count: number;
}

export interface RecentInterview {
  id: string;
  title: string;
  jobRole: string;
  status: string;
  overallScore?: number;
  createdAt: string;
}

export interface WeakArea {
  topic: string;
  occurrences: number;
  averageScore: number;
}

export interface Dashboard {
  totalInterviews: number;
  averageScore: number;
  highestScore: number;
  improvementPercentage: number;
  totalPracticeHours: number;
  scoreTrend: ScoreTrend[];
  skillPerformance: SkillPerformance[];
  recentInterviews: RecentInterview[];
  weakAreas: WeakArea[];
}

export interface AdminDashboard {
  totalUsers: number;
  activeUsers: number;
  totalInterviews: number;
  totalResumes: number;
  interviewsThisMonth: number;
  newUsersThisMonth: number;
  topSkills: { skill: string; count: number }[];
  recentUsers: AdminUser[];
}

export interface AdminUser {
  id: string;
  fullName: string;
  email: string;
  isActive: boolean;
  totalInterviews: number;
  averageScore: number;
  createdAt: string;
}

// API Response wrapper
export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
}
