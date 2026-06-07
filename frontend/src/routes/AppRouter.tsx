import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ProtectedRoute, AdminRoute, GuestRoute } from './ProtectedRoute';
import { MainLayout } from '../layouts/MainLayout';
import { AuthLayout } from '../layouts/AuthLayout';

// Pages — lazy load for performance
import { LoginPage } from '../pages/LoginPage';
import { RegisterPage } from '../pages/RegisterPage';
import { DashboardPage } from '../pages/DashboardPage';
import { ResumePage } from '../pages/ResumePage';
import { CreateInterviewPage } from '../pages/CreateInterviewPage';
import { InterviewRoomPage } from '../pages/InterviewRoomPage';
import { InterviewHistoryPage } from '../pages/InterviewHistoryPage';
import { InterviewResultsPage } from '../pages/InterviewResultsPage';
import { RoadmapPage } from '../pages/RoadmapPage';
import { AdminPage } from '../pages/AdminPage';
import { LandingPage } from '../pages/LandingPage';

export function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Landing */}
        <Route path="/" element={<LandingPage />} />

        {/* Guest only */}
        <Route element={<GuestRoute />}>
          <Route element={<AuthLayout />}>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
          </Route>
        </Route>

        {/* Protected */}
        <Route element={<ProtectedRoute />}>
          <Route element={<MainLayout />}>
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/resume" element={<ResumePage />} />
            <Route path="/interview/new" element={<CreateInterviewPage />} />
            <Route path="/interview/history" element={<InterviewHistoryPage />} />
            <Route path="/interview/:id/room" element={<InterviewRoomPage />} />
            <Route path="/interview/:id/results" element={<InterviewResultsPage />} />
            <Route path="/roadmap" element={<RoadmapPage />} />
          </Route>
        </Route>

        {/* Admin only */}
        <Route element={<AdminRoute />}>
          <Route element={<MainLayout />}>
            <Route path="/admin" element={<AdminPage />} />
          </Route>
        </Route>

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
