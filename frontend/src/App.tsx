import { ConfigProvider, theme } from 'antd';
import { AuthProvider } from './services/AuthContext';
import { AppRouter } from './routes/AppRouter';
import './index.css';

const darkTheme = {
  algorithm: theme.darkAlgorithm,
  token: {
    colorPrimary: '#6C63FF',
    colorBgBase: '#0f0c29',
    colorTextBase: '#ffffff',
    borderRadius: 10,
    fontFamily: '"Inter", "SF Pro Display", -apple-system, BlinkMacSystemFont, sans-serif',
    colorBgContainer: 'rgba(255,255,255,0.03)',
    colorBorder: 'rgba(255,255,255,0.1)',
    colorBorderSecondary: 'rgba(255,255,255,0.06)',
    colorFill: 'rgba(255,255,255,0.04)',
    colorFillSecondary: 'rgba(255,255,255,0.06)',
  },
  components: {
    Table: {
      colorBgContainer: 'transparent',
      headerBg: 'rgba(255,255,255,0.03)',
      rowHoverBg: 'rgba(108,99,255,0.06)',
      borderColor: 'rgba(255,255,255,0.06)',
    },
    Card: {
      colorBgContainer: 'rgba(255,255,255,0.03)',
      colorBorderSecondary: 'rgba(255,255,255,0.07)',
    },
    Menu: {
      itemColor: 'rgba(255,255,255,0.6)',
      itemHoverColor: '#ffffff',
      itemSelectedColor: '#ffffff',
      itemSelectedBg: 'rgba(108,99,255,0.15)',
      itemHoverBg: 'rgba(255,255,255,0.04)',
      subMenuItemBg: 'transparent',
    },
    Select: {
      colorBgContainer: 'rgba(255,255,255,0.05)',
      colorBgElevated: '#1a1650',
    },
    Input: {
      colorBgContainer: 'rgba(255,255,255,0.05)',
    },
    Modal: {
      contentBg: '#1a1650',
      headerBg: '#1a1650',
    },
    Collapse: {
      contentBg: 'rgba(255,255,255,0.02)',
      headerBg: 'transparent',
    },
    Tag: {
      defaultColor: 'rgba(255,255,255,0.8)',
      defaultBg: 'rgba(255,255,255,0.06)',
    },
  },
};

export default function App() {
  return (
    <ConfigProvider theme={darkTheme}>
      <AuthProvider>
        <AppRouter />
      </AuthProvider>
    </ConfigProvider>
  );
}
