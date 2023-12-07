'use client';

import { Slogan } from '@/app/components/slogan';
import ProfileSelect from '@/app/components/profileSelect';
import { Typography } from 'antd';

const { Title } = Typography;

const Profile = () => {
  return (
    <>
      <Slogan />
      <Title level={3} className='text-center mb-4'>Izvēlies profilu</Title>
      <ProfileSelect showButtons={true} width='100%' />
    </>
  );
}

export { Profile };
